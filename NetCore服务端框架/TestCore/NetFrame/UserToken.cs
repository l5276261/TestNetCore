using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NetFrame
{
    /// <summary>用户连接信息对象 </summary>
    public class UserToken
    {
        /// <summary>用户连接 </summary>
        public Socket conn;
        /// <summary>用户异步接收网络数据对象 </summary>
        public SocketAsyncEventArgs ReceiveSAEA;
        /// <summary>用户异步发送网络数据对象 </summary>
        public SocketAsyncEventArgs SendSAEA;
        public LengthEncode LE;
        public LengthDecode LD;
        public BodyEncode Encode;
        public BodyDecode Decode;
        public static NetType Type;

        public delegate void SendProcessDelegate(SocketAsyncEventArgs e);
        public SendProcessDelegate SendProcess;
        public delegate void CloseProcessDelegate(UserToken token, string error);
        public CloseProcessDelegate CloseProcess;
        public AbsHandlerCenter Center;

        List<byte> cache = new List<byte>();
        private bool isReading = false;
        Queue<byte[]> WriteQueue = new Queue<byte[]>();
        private bool isWriting = false;
        public UserToken() {
            ReceiveSAEA = new SocketAsyncEventArgs();
            ReceiveSAEA.UserToken = this;
            SendSAEA = new SocketAsyncEventArgs();
            SendSAEA.UserToken = this;
            //设置接收对象的缓冲区大小
            ReceiveSAEA.SetBuffer(new byte[1024], 0, 1024);
        }
        /// <summary>网络消息到达 </summary>
        public void Receive(byte[] buff) {
            //将消息写入缓存
            cache.AddRange(buff);
            if (!isReading) {
                isReading = true;
                OnData();
            }
        }
        /// <summary>缓存中有数据处理 </summary>
        public void OnData() {
            //解码消息存储对象
            byte[] buff = null;
            //当粘包解码器存在的时候，进行粘包处理
            if (LD != null) {
                buff = LD(ref cache);
                //消息未接收全，退出数据处理，等待下次消息到达
                if (buff == null) {
                    isReading = false; return;
                }
            } else {
                //缓存区中没有数据，直接退出消息处理，等待下次消息到达，防止尾递归造成的没有数据还解析的错误
                if (cache.Count == 0) {
                    isReading = false;return;
                }
                buff = cache.ToArray();
                cache.Clear();
            }
            //反序列化方法是否存在
            if (Decode == null) {
                throw new Exception("message decode process is null");
            }
            //进行消息反序列化
            object message = Decode(buff);
            //通知应用层有消息到达
            Center.MessageReceive(this, message);
            //尾递归，防止在消息处理过程中，有其他消息到达而没有经过处理
            OnData();
        }
        public void Write(byte[] value) {
            if (conn == null) {
                //此连接已经断开了
                CloseProcess(this, "调用已经断开的连接");
                return;
            }
            if (Type == NetType.KCP) {
                Send(value);
                return;
            }
            WriteQueue.Enqueue(value);
            if (!isWriting) {
                isWriting = true;
                OnWrite();
            }
        }
        public void OnWrite() {
            //判断发送消息队列是否有消息
            if (WriteQueue.Count == 0) {
                isWriting = false;return;
            }
            //取出第一条待发消息
            byte[] buff = WriteQueue.Dequeue();
            //设置消息发送异步对象的发送数据缓冲区数据
            SendSAEA.SetBuffer(buff, 0, buff.Length);
            //开启异步发送，UDP和TCP的发送API不同
            if (Type==NetType.TCP) {
                bool result = conn.SendAsync(SendSAEA);
                //是否挂起
                if (!result) {
                    SendProcess(SendSAEA);
                }
            } else if(Type==NetType.UDP) {
                bool result = conn.SendToAsync(SendSAEA);
                //是否挂起
                if (!result) {
                    SendProcess(SendSAEA);
                }
            }
        }
        public void Writed() {
            //与OnData尾递归同理
            OnWrite();
        }
        public void Close() {
            try {
                WriteQueue.Clear();
                cache.Clear();
                isReading = false;
                isWriting = false;
                if (Type==NetType.TCP) {
                    conn.Shutdown(SocketShutdown.Both);
                    conn.Close();
                }
                conn = null;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        #region UDP专用
        /// <summary>检查发送队列里面有没有数据，方便外部调用判断时候继续写数据还是归还token给池子，
        /// 暂时测试用，还没有做UDP的token维持。
        /// </summary>
        public bool CheckWriteQueue() {
            return WriteQueue.Count > 0;
        }
        #endregion
        #region KCP专用
        private static readonly DateTime utc_time = new DateTime(1970, 1, 1);

        public static UInt32 iclock() {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(utc_time).TotalMilliseconds) & 0xffffffff);
        }
        private bool isRun = false;
        private Action<byte[]> evHandler;
        private KCP kcp;
        private bool mNeedUpdateFlag;
        private UInt32 mNextUpdateTime;

        private SwitchQueue<byte[]> mRecvQueue = new SwitchQueue<byte[]>(128);

        public void init_kcp(UInt32 conv) {
            kcp = new KCP(conv, UdpSend);
            //快速模式
            kcp.NoDelay(1, 10, 2, 1);
            kcp.WndSize(128, 128);
        }
        public void Run() {
            isRun = true; evHandler = AfterKCPDecode;
        }
        /// <summary>网络消息到达 </summary>
        public void KcpReceive(byte[] buff) {
            //KCP分发：读取KCP消息头确定conv这个连接ID，好分配相同conv的KCP对象去处理这个消息
            UInt32 conv = LengthEncoding.DecodeKCP_ID(buff);
            mRecvQueue.Push(buff);
            //测试调用位置更改，貌似没区别，不用强制接收和发送在一个方法执行。
            //process_recv_queue();
        }
        /// <summary>
        /// 用于在KCP对接收到并处理后的完整数据包的基本的反序列化，不包含消息体的反序列化，
        /// 消息体反序列化由使用者自己决定如何反序列化
        /// </summary>
        public void AfterKCPDecode(byte[] data) {
            //解码消息存储对象
            byte[] result = null;
            List<byte> cache1 = new List<byte>();
            cache1.AddRange(data);
            if (LD != null) {
                //长度解码
                result = LD(ref cache1);
                //长度解码返回空 说明消息体不全，等待下条消息过来补全
                if (result == null) {
                    return;
                }
            }
            object message = Decode(result);
            if (message == null) {
                return;
            }
            //通知应用层有消息到达
            Center.MessageReceive(this, message);
        }
        /// <summary>KCP对象里面填充要发送的数据 </summary>
        public void Send(byte[] buf) {
            kcp.Send(buf);
            mNeedUpdateFlag = true;
        }
        public void Send(string str) {
            Send(System.Text.ASCIIEncoding.ASCII.GetBytes(str));
        }
        /// <summary>底层UDP发送供KCP调用 </summary>
        private void UdpSend(byte[] data, int size) {
            if (data == null) return;
            //设置消息发送异步对象的发送数据缓冲区数据
            SendSAEA.SetBuffer(data, 0, size);
            //开启异步发送
            bool result = conn.SendToAsync(SendSAEA);
            //是否挂起
            if (!result) {
                SendProcess(SendSAEA);
            }
        }
        
        /// <summary>主线程驱动 </summary>
        public void Update() {
            if (isRun) {
                update(iclock());
            }
        }
        void process_recv_queue() {
            mRecvQueue.Switch();

            while (!mRecvQueue.Empty()) {
                var buf = mRecvQueue.Pop();

                int ret = kcp.Input(buf);
                //收到的不是一个正确的KCP包
                if (ret < 0) {
                    if (evHandler != null) {
                        evHandler(buf);
                    }
                    return;
                }
                mNeedUpdateFlag = true;

                for (var size = kcp.PeekSize(); size > 0; size = kcp.PeekSize()) {
                    var buffer = new byte[size];
                    if (kcp.Recv(buffer) > 0) {
                        evHandler(buffer);
                    }
                }
            }
        }
        /// <summary>更新中检查时钟然后决定发送与否，先执行接收数据的处理 </summary>
        void update(UInt32 current) {
            process_recv_queue();

            if (mNeedUpdateFlag || current >= mNextUpdateTime) {
                kcp.Update(current);
                mNextUpdateTime = kcp.Check(current);
                mNeedUpdateFlag = false;
            }
        }
        #endregion
    }
}

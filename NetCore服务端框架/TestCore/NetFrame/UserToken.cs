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
        public string ID;
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
            CreateNewID();
            ReceiveSAEA = new SocketAsyncEventArgs();
            ReceiveSAEA.UserToken = this;
            SendSAEA = new SocketAsyncEventArgs();
            SendSAEA.UserToken = this;
            //设置接收对象的缓冲区大小
            ReceiveSAEA.SetBuffer(new byte[1024], 0, 1024);
        }
        /// <summary>重新生成ID </summary>
        public void CreateNewID() {
            ID = NetTool.GenerateGuidID();
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
            Console.WriteLine("发送时间 " + DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
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
                conn.Shutdown(SocketShutdown.Both);
                conn.Close();
                conn = null;
                //UserToken token = null;
                //TokenManager.Tcp_TokenDic.TryRemove(this.ID, out token);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        #region UDP专用
        public UdpServer UDPServer;
        /// <summary>网络消息到达 </summary>
        public void UdpReceive(byte[] buff) {
            //将消息写入缓存
            cache.AddRange(buff);
            if (!isReading) {
                isReading = true;
                UdpOnData();
            }
        }
        /// <summary>缓存中有数据处理 </summary>
        public void UdpOnData() {
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
                    isReading = false; return;
                }
                buff = cache.ToArray();
                cache.Clear();
            }
            //TODO 解析消息得到tokenID然后把数据放到制定的Token去执行，没有Token就取出Token然后放入执行。
            //测试，假设得到了tokenID
            string id = "adafd";
            UserToken token = null;
            if (!TokenManager.Udp_TokenDic.ContainsKey(id)) {
                if (UDPServer.pool.TryPop(out token)) {
                    token.ID = id; token.conn = this.conn; token.SendSAEA = this.SendSAEA;token.SendSAEA.UserToken = token;
                    TokenManager.Udp_TokenDic.TryAdd(id, token);
                }
            } else token = TokenManager.Udp_TokenDic[id];


            //反序列化方法是否存在
            if (Decode == null) {
                throw new Exception("message decode process is null");
            }
            //进行消息反序列化
            object message = Decode(buff);
            //通知应用层有消息到达
            Center.MessageReceive(token, message);
            //尾递归，防止在消息处理过程中，有其他消息到达而没有经过处理
            OnData();
        }
        public void UdpClose() {
            try {
                WriteQueue.Clear();
                cache.Clear();
                isReading = false;
                isWriting = false;
                conn = null;

                UserToken token = null;
                TokenManager.Udp_TokenDic.TryRemove(this.ID, out token);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        #endregion
        #region KCP专用
        public KcpServer KCPServer;
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
            //Internet上的标准MTU值为576，所以Internet的UDP编程时数据长度最好在576－20－8＝548字节以内，已经由KCP自行处理了。
            kcp.SetMtu(548);
            kcp.FastSet();
        }
        public void Run() {
            isRun = true; evHandler = AfterKCPDecode;
        }
        /// <summary>网络消息到达 </summary>
        public void KcpReceive(byte[] buff) {
            //KCP分发：读取KCP消息头确定conv这个连接ID，好分配相同conv的KCP对象去处理这个消息
            UInt32 conv = LengthEncoding.DecodeUInt32(buff,0);
            //0代表是第一次连接服务端，还没有分配KCP
            if (conv == 0) {

                UserToken token = null;
                if (KCPServer.pool.TryPop(out token)) {
                    token.conn = this.conn; token.SendSAEA = this.SendSAEA; token.SendSAEA.UserToken = token;
                    //初始化KCP，这里开启就是动态开启，改为在TOKEN初始化的时候进行初始化。
                    //token.init_kcp(1);
                    //运行KCP
                    token.Run();
                    if (TokenManager.Kcp_TokenDic.TryAdd(token.kcp.GetConv(), token)) {
                        //TokenManager.Kcp_TokenList.Add(token);
                    }
                }
                //通知客户端KCP的conv编号
                UdpSend(SerializeUtil.UintToBytes(token.kcp.GetConv()), 4);

            } else {
                TokenManager.Kcp_TokenDic[conv].mRecvQueue.Push(buff);
                //测试调用位置更改，不用强制接收和发送在一个方法执行。
                //放到这里比较好，可以让正常情况下收发信息的时间差在10毫秒左右，强制接收和发送在一个方法执行会导致时间差在15毫秒左右并且一般不低于10。
                TokenManager.Kcp_TokenDic[conv].process_recv_queue();
            }
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
            //Console.WriteLine("解析消息时间 " + DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
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
            //Console.WriteLine("放进消息队列时间 " + DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        }
        public void Send(string str) {
            Send(System.Text.ASCIIEncoding.ASCII.GetBytes(str));
        }
        /// <summary>底层UDP发送供KCP调用 </summary>
        private void UdpSend(byte[] data, int size) {
            if (data == null) return;
            //不能用UDP直接发送一个大于SAEA接收buffer的数据包，
            //因为UDP接收到的数据跟TCP不一样，TCP会超过buffer大小会自动根据这个大小一个一个的发过来，UDP不会，他会直接把你指定大小的发过去，即不会替你分包的。
            //TCP会，KCP算法也会，会多次发送他分开的包，然后整合成一个完整的包，他其实是另一版本的TCP。
            //网络数据的单个包（TCP会自动根据MTU分成多个数据包，UDP就是一次发送的数据的大小，只能自己分包或者类似KCP算法分包），也就是相当于有MTU的限制和不同环境的MTU，
            //最好是设置Internet上的标准MTU值为576，Internet的UDP编程时数据长度最好在576－20－8＝548字节以内，和KCP的MTU是两码事，KCP的MTU是他允许的最大单包数据，
            //包含他定义的消息头等和我们自己的有效数据。
            //设置消息发送异步对象的发送数据缓冲区数据，连续发送的时候会出问题，因为该SAEA已经有个异步操作还在执行，方案每次发送使用不同的SAEA，或者直接用同步发送,
            //是在用KCP发送大于设置的KCP的MTU小于SAEA接收的buffer大小的情况下，发现的这个UDP错误提示。
            //SendSAEA.SetBuffer(data, 0, size);
            //开启异步发送
            //bool result = conn.SendToAsync(SendSAEA);
            ////是否挂起
            //if (!result) {
            //    SendProcess(SendSAEA);
            //}
            SendSAEA.SetBuffer(data, 0, size);
            Console.WriteLine("发送时间 " + DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
            bool result = conn.SendToAsync(SendSAEA);
            //是否挂起
            if (!result) {
                SendProcess(SendSAEA);
            }
            //conn.SendTo(data, size, SocketFlags.None, SendSAEA.RemoteEndPoint);
        }
        
        /// <summary>主线程驱动 </summary>
        public void Update() {
            if (isRun) {
                update(iclock());
            }
        }
        public void process_recv_queue() {
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
            //process_recv_queue();

            if (mNeedUpdateFlag || current >= mNextUpdateTime) {
                kcp.Update(current);
                mNextUpdateTime = kcp.Check(current);
                mNeedUpdateFlag = false;
            }
        }
        public void KcpClose() {
            try {
                WriteQueue.Clear();
                cache.Clear();
                isReading = false;
                isWriting = false;
                conn = null;

                UserToken token = null;
                if (TokenManager.Kcp_TokenDic.TryRemove(this.kcp.GetConv(), out token)) {
                    //int index = TokenManager.Kcp_TokenList.FindIndex(m => m.kcp.GetConv() == token.kcp.GetConv());
                    //if (index != -1) TokenManager.Kcp_TokenList.RemoveAt(index);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}

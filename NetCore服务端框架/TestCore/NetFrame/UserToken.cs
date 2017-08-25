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
        public static bool IsUdp;

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
            if (!IsUdp) {
                bool result = conn.SendAsync(SendSAEA);
                //是否挂起
                if (!result) {
                    SendProcess(SendSAEA);
                }
            } else {
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
                if (!IsUdp) {
                    conn.Shutdown(SocketShutdown.Both);
                    conn.Close();
                }
                conn = null;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        #region UDP专用
        /// <summary>检查发送队列里面有没有数据 </summary>
        public bool CheckWriteQueue() {
            return WriteQueue.Count > 0;
        }
        #endregion
    }
}

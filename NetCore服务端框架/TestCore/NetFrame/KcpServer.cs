using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetFrame
{
    public class KcpServer {
        Socket server;//服务器socket监听对象
        private SocketAsyncEventArgs receiveSocketArgs;//接收用socket异步事件
        private Socket sendSocket;//用于发送的socket
        int maxClient;//最大客户端连接数
        Semaphore acceptClients;
        UserTokenPool pool;
        public LengthEncode LE;
        public LengthDecode LD;
        public BodyEncode Encode;
        public BodyDecode Decode;
        /// <summary>消息中心，由外部应用传入 </summary>
        public AbsHandlerCenter Center;
        UserToken kcpToken;
        /// <summary>初始化通信监听，port：监听端口 </summary>
        public KcpServer(int max) {
            //IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            //实例化监听对象
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //设定服务器最大连接人数
            maxClient = max;
        }
        /// <summary>开启服务端 </summary>
        public void Start(int port) {
            //创建连接池
            pool = new UserTokenPool(maxClient);
            //连接信号量
            acceptClients = new Semaphore(maxClient, maxClient);
            for (int i = 0; i < maxClient; i++) {
                UserToken token = new UserToken();
                //初始化token信息
                token.ReceiveSAEA = null;
                token.SendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
                token.LD = LD;
                token.LE = LE;
                token.Encode = Encode;
                token.Decode = Decode;
                token.SendProcess = ProcessSend;
                token.CloseProcess = ClientClose;
                token.Center = Center;
                pool.Push(token);
            }
            receiveSocketArgs = new SocketAsyncEventArgs();
            receiveSocketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
            receiveSocketArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            receiveSocketArgs.SetBuffer(new byte[1024], 0, 1024);
            #region KCP测试用
            kcpToken = pool.Pop();
            kcpToken.init_kcp(1);kcpToken.Run();
            TokenManager.TokenDic.Add(1, kcpToken);
            #endregion
            //监听当前服务器网卡所有可用IP地址的port端口
            //外网IP 内网IP192.168.x.x 本机IP一个127.0.0.1
            try {
                server.Bind(new IPEndPoint(IPAddress.Any, port));
                sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //置于监听状态，并没有真正的有监听事件
                //server.Listen(10);
                StartReceive();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        public void StartReceive() {
            //用户连接对象，开启异步数据接收
            bool result = server.ReceiveFromAsync(receiveSocketArgs);
            //异步事件是否挂起
            if (!result) {
                ProcessReceive(receiveSocketArgs);
            }
        }
        public void IOCompleted(object sender, SocketAsyncEventArgs e) {
            try {
                if (e.LastOperation == SocketAsyncOperation.ReceiveFrom) {
                    ProcessReceive(e);
                } else {
                    ProcessSend(e);
                }
            } catch (Exception E) {
                Console.WriteLine("IO_Completed {0} error, message: {1}", e.ConnectSocket.LocalEndPoint, E.Message);
            }
        }
        public void ProcessReceive(SocketAsyncEventArgs e) {
            UserToken token = kcpToken; token.conn = sendSocket; token.SendSAEA.RemoteEndPoint = e.RemoteEndPoint;
            //判断网络消息接收是否成功
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success) {
                byte[] message = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, 0, message, 0, e.BytesTransferred);
                //处理接收到的消息
                token.KcpReceive(message);
                StartReceive();
            } else {
                if (e.SocketError != SocketError.Success) {
                    ClientClose(token, e.SocketError.ToString());
                } else {
                    //消息长度为0就认为客户端主动断开连接
                    ClientClose(token, "客户端主动断开连接");
                }
            }
        }
        public void ProcessSend(SocketAsyncEventArgs e) {
            UserToken token = e.UserToken as UserToken;
            if (e.SocketError != SocketError.Success) {
                ClientClose(token, e.SocketError.ToString());
            } else {
                //消息发送成功，回调成功
                //token.Writed();
            }
        }
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="token">断开连接的用户对象</param>
        /// <param name="error">断开连接的错误编码</param>
        public void ClientClose(UserToken token, string error) {
            if (token.conn != null) {
                //防止关闭释放的时候，出现多线程的访问，也是避免同一个userToken同时有多个线程操作
                lock (token) {
                    //通知应用层面，客户端断开连接了
                    Center.ClientClose(token, error);
                    token.Close();
                    pool.Push(token);
                }
            }
        }
    }
}

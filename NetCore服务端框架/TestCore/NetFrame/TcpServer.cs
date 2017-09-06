using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetFrame
{
    public class TcpServer
    {
        Socket server;//服务器socket监听对象
        int maxClient;//最大客户端连接数
        Semaphore acceptClients;
        ConcurrentStack<UserToken> pool;
        public LengthEncode LE;
        public LengthDecode LD;
        public BodyEncode Encode;
        public BodyDecode Decode;
        /// <summary>消息中心，由外部应用传入 </summary>
        public AbsHandlerCenter Center;

        /// <summary>初始化通信监听，port：监听端口 </summary>
        public TcpServer(int max) {
            //IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            //实例化监听对象
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //设定服务器最大连接人数
            maxClient = max;
        }
        /// <summary>开启服务端 </summary>
        public void Start(int port) {
            //创建连接池
            pool = new ConcurrentStack<UserToken>();
            //连接信号量
            acceptClients = new Semaphore(maxClient, maxClient);
            for (int i = 0; i < maxClient; i++) {
                UserToken token = new UserToken();
                //初始化token信息，可以一开始初始化一定量放入池子，也可以在需要取用的时候进行初始化放入池子。
                token.ReceiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IOCompleted);
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
            //监听当前服务器网卡所有可用IP地址的port端口
            //外网IP 内网IP192.168.x.x 本机IP一个127.0.0.1
            try {
                server.Bind(new IPEndPoint(IPAddress.Any, port));
                //置于监听状态，并没有真正的有监听事件
                server.Listen(10);
                StartAccept(null);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>开始客户端连接监听 </summary>
        public void StartAccept(SocketAsyncEventArgs e) {
            //如果当前传入为空，说明调用新的客户端连接监听事件，否则移除当前客户端连接
            if (e == null) {
                e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
            } else {
                e.AcceptSocket = null;
            }
            //信号量-1
            acceptClients.WaitOne();
            bool result = server.AcceptAsync(e);
            //判断异步事件是否挂起，若没有，说明立刻执行完成，直接处理事件，否则会才处理完成后出发AcceptCompleted事件
            if (!result) {
                ProcessAccept(e);
            }
        }
        public void ProcessAccept(SocketAsyncEventArgs e) {
            //从连接对象池取出连接对象，供新用户使用
            UserToken token;
            if (!pool.TryPop(out token)) {
                Console.WriteLine("没有足够的token");return;
            }
            token.conn = e.AcceptSocket;
            //TokenManager.Tcp_TokenDic.TryAdd(token.ID, token);
            //通知应用层有客户端连接
            Center.ClientConnect(token);
            //开启消息到达监听
            StartReceive(token);
            //释放当前异步对象
            StartAccept(e);
        }
        public void AcceptCompleted(object sender, SocketAsyncEventArgs e) {
            ProcessAccept(e);
        }
        public void StartReceive(UserToken token) {
            //用户连接对象，开启异步数据接收
            bool result = token.conn.ReceiveAsync(token.ReceiveSAEA);
            //异步事件是否挂起
            if (!result) {
                ProcessReceive(token.ReceiveSAEA);
            }
        }
        public void IOCompleted(object sender, SocketAsyncEventArgs e) {
            UserToken token = e.UserToken as UserToken;
            try {
                //避免同一个userToken同时有多个线程操作，使该用户对象逻辑处理都是串行的，
                //当然也可以有其他方式比如数据进入队列然后串行操作，暂时先这样，业务逻辑设计的时候再改造  
                //不用加锁，消息执行对于自己来说是串行的
                //lock (token) {
                    if (e.LastOperation == SocketAsyncOperation.Receive) {
                        ProcessReceive(e);
                    } else {
                        ProcessSend(e);
                    }
                //}
            } catch (Exception E) {
               Console.WriteLine("IO_Completed {0} error, message: {1}", token.conn.LocalEndPoint, E.Message);
            }
        }
        public void ProcessReceive(SocketAsyncEventArgs e) {
            UserToken token = e.UserToken as UserToken;
            //判断网络消息接收是否成功
            if (token.ReceiveSAEA.BytesTransferred > 0 && token.ReceiveSAEA.SocketError == SocketError.Success) {
                Console.WriteLine("接收到时间 "+DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
                byte[] message = new byte[token.ReceiveSAEA.BytesTransferred];
                Buffer.BlockCopy(token.ReceiveSAEA.Buffer, 0, message, 0, token.ReceiveSAEA.BytesTransferred);
                //处理接收到的消息
                token.Receive(message);
                StartReceive(token);
            } else {
                if (token.ReceiveSAEA.SocketError != SocketError.Success) {
                    ClientClose(token, token.ReceiveSAEA.SocketError.ToString());
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
                token.Writed();
            }
        }
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="token">断开连接的用户对象</param>
        /// <param name="error">断开连接的错误编码</param>
        public void ClientClose(UserToken token,string error) {
            if (token.conn != null) {
                //防止关闭释放的时候，出现多线程的访问，也是避免同一个userToken同时有多个线程操作，比如发送的时候失败在关闭，结果收到了消息
                lock (token) {
                    //通知应用层面，客户端断开连接了
                    Center.ClientClose(token, error);
                    token.Close();
                    pool.Push(token);
                    //加回一个信号量，供其他用户使用
                    acceptClients.Release();
                }
            }
        }
    }
}

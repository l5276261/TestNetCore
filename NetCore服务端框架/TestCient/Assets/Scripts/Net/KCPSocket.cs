using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class KCPSocket : InstanceNormal<KCPSocket> {
    private static readonly DateTime utc_time = new DateTime(1970, 1, 1);

    public static UInt32 iclock() {
        return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(utc_time).TotalMilliseconds) & 0xffffffff);
    }

    private Socket socket;
    private string ip = "116.62.233.121";//"192.168.2.3";//"127.0.0.1";//"116.62.233.121";
    private int port = 6666;
    private bool isRun = false;
    private bool isConnect = false;
    private EndPoint server;
    private EndPoint receiveServer;
    private byte[] readBuff = new byte[1024*2];
    private Action<byte[]> evHandler;
    private KCP mKcp;
    private bool mNeedUpdateFlag;
    private UInt32 mNextUpdateTime;
    public List<SocketModel> messages = new List<SocketModel>();

    private SwitchQueue<byte[]> mRecvQueue = new SwitchQueue<byte[]>(128);

    public KCPSocket() {
        evHandler = AfterKCPDecode;
        SerializeUtil.IsPBOrJson = false;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        server = new IPEndPoint(IPAddress.Parse(ip), port);
        receiveServer = new IPEndPoint(IPAddress.Parse(ip), port);
        //直接初始化一个conv的KCP，实际应该是由服务端分配。
        //init_kcp((UInt32)new System.Random((int)DateTime.Now.Ticks).Next(1, Int32.MaxValue));
        init_kcp(1);isConnect = true;
        try {
            //开启异步数据接收
            socket.BeginReceiveFrom(readBuff, 0, 1024, SocketFlags.None, ref receiveServer, new AsyncCallback(ReceiveCallBack), receiveServer);
            isRun = true;
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
        //连接服务端
        //ConnectServer();
    }

    void init_kcp(UInt32 conv) {
        mKcp = new KCP(conv,UdpSend);
        //快速模式
        mKcp.NoDelay(1, 10, 2, 1);
        mKcp.WndSize(128, 128);
        //Internet上的标准MTU值为576，所以Internet的UDP编程时数据长度最好在576－20－8＝548字节以内
        mKcp.SetMtu(548);
        mKcp.FastSet();
    }
    /// <summary>异步接收回调 </summary>
    private void ReceiveCallBack(IAsyncResult iar) {
        if (!isRun) return;
        //Debug.Log("接收到时间 "+ DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        try {
            //获取当前收到的消息长度
            int length = socket.EndReceiveFrom(iar, ref receiveServer);
            if (length>0) {
                byte[] message = new byte[length];
                Buffer.BlockCopy(readBuff, 0, message, 0, length);
                if (!isConnect) {
                    UInt32 conv = SerializeUtil.BytesToUint(message,0);
                    init_kcp(conv);isConnect = true;
                } else
                    // push udp packet to switch queue.
                    mRecvQueue.Push(message);
            }
            //尾递归 再次开启异步消息接收 消息到达后会直接写入 缓冲区 readbuff
            socket.BeginReceiveFrom(readBuff, 0, readBuff.Length, SocketFlags.None, ref receiveServer, new AsyncCallback(ReceiveCallBack), receiveServer);
        } catch (Exception e) {
            Debug.Log("远程服务器主动断开连接 " + e.Message);
            Close();
        }
    }
    /// <summary>连接服务端，并获取服务端分配的KCP的conv编号 </summary>
    public void ConnectServer() {
        if (!isConnect)
            UdpSend(SerializeUtil.UintToBytes(0), 4);
    }
    /// <summary>
    /// 用于在KCP对接收到并处理后的完整数据包的基本的反序列化，不包含消息体的反序列化，
    /// 消息体反序列化由使用者自己决定如何反序列化
    /// </summary>
    public void AfterKCPDecode(byte[] data) {
        Debug.Log("解析数据时间 "+DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        List<byte> cache = new List<byte>();
        cache.AddRange(data);
         //长度解码
        byte[] result = LengthEncoding.Decode(ref cache);
        //长度解码返回空 说明消息体不全，等待下条消息过来补全
        if (result == null) {
            return;
        }
        SocketModel message = MessageEncoding.Decode(result) as SocketModel;
        if (message == null) {
            return;
        }
        //进行消息的处理
        messages.Add(message);
    }
    //序列化数据并且转换成发送的消息，然后发送
    public void Write(byte type, int area, int command, object message) {
        //if (!isConnect) ConnectServer();
        //发送消息
        Send(Encode(type, area, command, message));
    }
    /// <summary>编码数据成二进制协议</summary>
    public byte[] Encode(byte type, int area, int command, object message) {
        SocketModel model = new SocketModel() { Type = type, Area = area, Command = command, Message = message };
        return LengthEncoding.Encode(MessageEncoding.Encode(model));
    }
    /// <summary>KCP对象里面填充要发送的数据 </summary>
    public void Send(byte[] buf) {
        //可以在此通过WaitSnd的数量合理性判断是否发送该数据，不过服务端是要通知给客户端的，不能丢包，这个判断对于客户端是否合理丢包的判断是可以的。
        //ikcp_send 前，先检查 ikcp_waitsnd，如果 waitsnd 大于某个阈值比如 64，则认为当前网络太差，然后不调用 ikcp_send，
        //丢弃这个数据(或者上层消息队列继续缓存这个数据，从王者的表现来看是直接丢弃的)。同时启动一个计时器，如果 waitsnd 持续 10s 都大于 64，
        //则认为网络奇差无比，根本无法游戏，销毁 kcpcb，断线处理。如果 10s 内 waitsnd 一旦小于 32，则认为网络恢复正常，取消掉前面的计时器。
        //Debug.Log(mKcp.WaitSnd());
        mKcp.Send(buf);
        mNeedUpdateFlag = true;
        Debug.Log("放进消息队列时间 "+DateTime.Now.Minute+" "+DateTime.Now.Second+" "+DateTime.Now.Millisecond);
    }

    public void Send(string str) {
        Send(System.Text.ASCIIEncoding.ASCII.GetBytes(str));
    }
    /// <summary>底层UDP发送供KCP调用 </summary>
    private void UdpSend(byte[] data, int size) {
        try {
            if (data == null) return;
            //Debug.Log("udpsend ");
            //if (mKcp != null) Debug.Log(mKcp.WaitSnd());
            //Debug.Log("发送消息时间 " + DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
            socket.SendTo(data, 0, size, SocketFlags.None, server);
            #region 异步发送
            //IAsyncResult send = socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, server, new AsyncCallback(SendCallBack), server);
            ////阻塞直到连接收到成功或者失败信号或者500毫秒后继续执行后面的
            //bool success = send.AsyncWaitHandle.WaitOne(500, true);
            //if (!success) {
            //    Debug.Log("send error close socket");
            //    Close();
            //    return;
            //} else Debug.Log("dadfafd");
            //isConnect = true;
            #endregion
        } catch (Exception e) {
            //如果异常错误比如服务端断掉，那么就不会执行发送的回调的。           
            Debug.Log("网络错误，请重新登录" + e.Message);
            Close();
        }
    }
    /// <summary>主线程驱动 </summary>
    public void Update() {
        if (isRun) {
            update(iclock());
        }
    }

    public void Close() {
        if (!isRun) return;
        isRun = false;
        isConnect = false;
        try {
            socket.Shutdown(SocketShutdown.Both);
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
        socket.Close();
    }

    void process_recv_queue() {
        mRecvQueue.Switch();

        while (!mRecvQueue.Empty()) {
            var buf = mRecvQueue.Pop();
            int ret = mKcp.Input(buf);
            //收到的不是一个正确的KCP包
            if (ret < 0) {
                if (evHandler != null) {
                    evHandler(buf);
                }
                return;
            }
            mNeedUpdateFlag = true;

            for (var size = mKcp.PeekSize(); size > 0; size = mKcp.PeekSize()) {
                var buffer = new byte[size];
                if (mKcp.Recv(buffer) > 0) {
                    evHandler(buffer);
                }
            }
        }
    }
    /// <summary>更新中检查时钟然后决定发送与否，先执行接收数据的处理 </summary>
    void update(UInt32 current) {
        if (!isConnect) return;
        process_recv_queue();

        if (mNeedUpdateFlag || current >= mNextUpdateTime) {
            mKcp.Update(current);
            mNextUpdateTime = mKcp.Check(current);
            mNeedUpdateFlag = false;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UdpClient :InstanceNormal<UdpClient> {
    private Socket socket;
    private string ip = "127.0.0.1";//"192.168.2.3";//"127.0.0.1";//"116.62.233.121";
    private int port = 6666;
    private bool isConnect = false;//UDP里面没有状态连接这个只是作为socket可否释放的一个判断
    private EndPoint server;
    private EndPoint receiveServer;
    private byte[] readBuff = new byte[1024];
    List<byte> cache = new List<byte>();
    public List<MessageModel> messages = new List<MessageModel>();
    private bool isReading = false;
    public UdpClient() {
        SerializeUtil.IsPBOrJson = false;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        server = new IPEndPoint(IPAddress.Parse(ip), port);
        receiveServer = new IPEndPoint(IPAddress.Parse(ip), port);
        try {
            //开启异步数据接收
            socket.BeginReceiveFrom(readBuff, 0, 1024, SocketFlags.None, ref receiveServer, new AsyncCallback(ReceiveCallBack), receiveServer);
            isConnect = true;
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }
    /// <summary>异步接收回调 </summary>
    private void ReceiveCallBack(IAsyncResult iar) {
        try {
            //获取当前收到的消息长度
            int length = socket.EndReceiveFrom(iar, ref receiveServer);
            byte[] message = new byte[length];
            Buffer.BlockCopy(readBuff, 0, message, 0, length);
            cache.AddRange(message);
            if (!isReading) {
                isReading = true;
                OnData();
            }
            //Debug.Log("消息数量 " + messages.Count);
            //尾递归 再次开启异步消息接收 消息到达后会直接写入 缓冲区 readbuff
            socket.BeginReceiveFrom(readBuff, 0, 1024, SocketFlags.None, ref receiveServer, new AsyncCallback(ReceiveCallBack), receiveServer);
        } catch (Exception e) {
            Debug.Log("远程服务器主动断开连接 " + e.Message);
            Close();
        }
    }
    /// <summary>缓存中有数据处理 </summary>
    void OnData() {
        //长度解码
        byte[] result = LengthEncoding.Decode(ref cache);

        //长度解码返回空 说明消息体不全，等待下条消息过来补全
        if (result == null) {
            isReading = false;
            return;
        }

        MessageModel message = MessageEncoding.Decode(result) as MessageModel;

        if (message == null) {
            isReading = false;
            return;
        }
        //进行消息的处理
        messages.Add(message);
        Debug.Log(DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        //尾递归 防止在消息处理过程中 有其他消息到达而没有经过处理
        OnData();
    }
    //序列化数据并且转换成发送的消息，然后发送
    public void Write(int id, object message) {
        //Debug.Log("Write " + isConnect);
        Debug.Log(DateTime.Now.Minute + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        //if (!isConnect) ConnectServer();
        //发送消息
        Send(Encode(id, message));
    }
    /// <summary>编码数据成二进制协议</summary>
    public byte[] Encode(int id, object message) {
        MessageModel model = new MessageModel() { ID=id, Message = message };
        #region 原本写法，感觉有问题
        //ByteArray ba = new ByteArray();
        //ba.write(type);
        //ba.write(area);
        //ba.write(command);
        ////判断消息体是否为空  不为空则序列化后写入
        //if (message != null) {
        //    ba.write(SerializeUtil.Encode(message));
        //}
        //ByteArray arr1 = new ByteArray();
        //arr1.write(ba.Length);
        //arr1.write(ba.getBuff());
        //return arr1.getBuff();
        #endregion
        return LengthEncoding.Encode(MessageEncoding.Encode(model));
    }
    /// <summary>发送消息给服务端，UDP同步发送或者异步+阻塞发送都不会卡住线程就是说他的发送就是立刻完成不会卡线程，
    /// 但是不代表数据包立刻或者一定到服务端，这就需要一定机制保证所有玩家看到的效果一致。按照帧同步策略来说，即使数据包未到也要用默认的空帧转发或者运算，
    /// 还有甚至加入本地差值平滑，比如玩家网络不好，自己的消息发送慢，然后其他玩家的数据也收的慢，最后导致其他玩家看到的他的数据是一致的，
    /// 他是之前的默认状态，但是他看到的其他玩家的状态因为未收到数据保持默认状态会跟其他玩家的真实状态有出入，之后他的数据到达服务端和服务端的数据到来，
    /// 他就要靠差值平滑来过渡中间的缺失，包括其他玩家的缺失和自己的缺失，这个有点类似状态同步的处理策略。。不过是帧同步默认执行的是上一次接收到的帧的操作命令，
    /// 然后接收到正确的操作命令后进行差值运算平滑补偿，减少卡顿的不舒服。
    /// </summary>
    private void Send(byte[] data) {
        //Debug.Log("UDP Send");
        try {
            if (data == null) return;
            socket.SendTo(data,server);
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
    #region 异步发送回调
    private void SendCallBack(IAsyncResult ar) {
        var sendOK = false;
        try {
            socket.EndSendTo(ar);
            if (ar.IsCompleted) {
                sendOK = true;
                Debug.Log("dadfafd");
            }
        } catch (Exception e) {
            Debug.Log("发送出错 " + e.Message);
            Close();
        }
    }
    #endregion
    public void Close() {
        //Debug.Log("进入Close "+isConnect);
        if (!isConnect) return;
        isConnect = false;
        //Debug.Log("开始ShutDown " + isConnect);
        socket.Shutdown(SocketShutdown.Both);
        //Debug.Log("开始SocketClose " + isConnect);
        socket.Close();
    }
}

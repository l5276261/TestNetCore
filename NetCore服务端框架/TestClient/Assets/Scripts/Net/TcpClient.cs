using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TcpClient : InstanceNormal<TcpClient> {
    private Socket socket;
    private string ip = "127.0.0.1";//"192.168.2.3";//"127.0.0.1";//"192.168.2.146";//"116.62.233.121";
    private int port = 6666;
    private bool isConnect = false;
    private byte[] readBuff = new byte[1024];
    List<byte> cache = new List<byte>();
    public List<MessageModel> messages = new List<MessageModel>();
    private bool isReading = false;
    public TcpClient() {
        SerializeUtil.IsPBOrJson = false;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }
    /// <summary>连接服务器同步或者异步+阻塞方式都会卡线程，尤其是网络不好的情况下，这个适合第一次重新连接服务器的情景</summary>
    public void ConnectServer() {
        try {
            if (HasNet()) {
                //连接到服务器
                //socket.Connect(ip, port);
                IAsyncResult connect = socket.BeginConnect(ip, port, new AsyncCallback(ConnectCallBack), socket);
                //阻塞直到连接收到成功或者失败信号或者500毫秒后继续执行后面的
                bool success = connect.AsyncWaitHandle.WaitOne(500, true);
                if (!success) {
                    Debug.Log("connect error close socket");
                    isConnect = false;
                    return;
                }
            } else {
                Debug.Log("客户端没有网络");
                isConnect = false;
            }
        } catch (Exception e) {
            //Connect同步连接的方法由于没有回调只能在这里面做连接失败的判断，异步的在回调里面和这里都可以判断，
            //连接比较适合同步写法或者异步 +阻塞线程的写法。
            //不管是服务端没有开启还是客户端网络断开，都会由于开始连接服务端失败报异常错误
            Debug.Log(e.Message);
            isConnect = false;
        }
    }
    private void ConnectCallBack(IAsyncResult ar) {
        try {
            socket.EndConnect(ar);
            isConnect = true;
            //连接成功，开始侦听接收
            //开启异步消息接收，消息会到达后直接写入缓冲区readBuff，最后一个参数无所谓传个空都行
            socket.BeginReceive(readBuff, 0, 1024, SocketFlags.None, ReceiveCallBack, readBuff);
        } catch (Exception e) {
            //不管是服务端没有开启还是客户端网络断开，都会由于开始连接服务端失败报异常错误
            Debug.Log(e.Message);
            isConnect = false;
        }
    }
    /// <summary>收到消息回调 </summary>
    private void ReceiveCallBack(IAsyncResult ar) {
        try {
            //获取当前收到的消息长度
            int length = socket.EndReceive(ar);
            byte[] message = new byte[length];
            Buffer.BlockCopy(readBuff, 0, message, 0, length);
            cache.AddRange(message);
            if (!isReading) {
                isReading = true;
                OnData();
            }
            //尾递归 再次开启异步消息接收 消息到达后会直接写入 缓冲区 readbuff
            socket.BeginReceive(readBuff, 0, 1024, SocketFlags.None, ReceiveCallBack, readBuff);
        } catch (Exception e) {
            //比如一开始连接上了，中间服务端断开了
            Debug.Log("远程服务器主动断开连接 "+e.Message);            
            Close();
        }
    }
    //缓存中有数据处理
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
        if (!isConnect) ConnectServer();
        //发送消息
        Send(Encode(id, message));
    }
    /// <summary>
    /// 编码数据成二进制协议
    /// </summary>
    /// <param name="type"></param>
    /// <param name="area"></param>
    /// <param name="command"></param>
    /// <param name="message"></param>
    public byte[] Encode(int id, object message) {
        MessageModel model = new MessageModel() { ID=id, Message = message };
        Debug.Log((LengthEncoding.Encode(MessageEncoding.Encode(model))).Length);
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
    /// <summary>发送消息给服务端，TCP同步发送或者异步+阻塞发送都会卡线程，尤其是网络不好的情况下，
    /// 这个根据需要是不是允许玩家网络卡的时候游戏继续运行，来决定不卡或者卡线程，默认设置为异步非阻塞发送
    /// </summary>
    private void Send(byte[] data) {
        try {
            if (!isConnect||data==null) return;
            //socket.Send(arr1.getBuff());
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
            //IAsyncResult send = socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
            //阻塞直到连接收到成功或者失败信号或者500毫秒后继续执行后面的
            //bool success = send.AsyncWaitHandle.WaitOne(500, true);
            //if (!success) {
            //    Debug.Log("send error close socket");
            //    Close();
            //    return;
            //}

        } catch (Exception e) {
            //如果异常错误比如服务端断掉，那么就不会执行发送的回调的。           
            Debug.Log("网络错误，请重新登录" + e.Message);
            Close();
        }
    }
    private void SendCallBack(IAsyncResult ar) {
        var sendOK = false;
        try {
            socket.EndSend(ar);
            if (ar.IsCompleted) {
                sendOK = true;
            }
        } catch (Exception e) {
            Debug.Log("发送出错 " + e.Message);
            Close();
        }
    }
    /// <summary>
    ///关闭socket但是不赋值为空，免得下次使用（比如断线重连）还要实例化一个，减少以后的重复实例化
    ///ReceiveCallBack里面异常调用的时候，即远程主机断开连接的时候，
    ///如果Close没有加isConnect=false，在执行后面的shutdown和close，
    ///是不会执行完成socket.Shutdown，只会执行到Shutdown之前的Log，然后关闭游戏的时候再次调用Close就会报错执行到socket.Shutdown的时候。
    /// </summary>
    public void Close() {
        //Debug.Log("进入Close "+isConnect);
        if (!isConnect) return;
        isConnect = false;
        //Debug.Log("开始ShutDown " + isConnect);
        socket.Shutdown(SocketShutdown.Both);
        //Debug.Log("开始SocketClose " + isConnect);
        socket.Close();
    }
    /// <summary>
    /// 代表客户端网络断开，意思是本机网络断开即没有网络，如果是本机上有服务端和客户端，那么是可以正常通信的，所以这个判断条件有点尴尬，
    /// 当然可以保证本机有网络的情况下，使用这个判断条件。
    /// </summary>
    private bool HasNet() {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum NetType { TCP,UDP,KCP}
public static class Ex {
    public static NetType Type;
    /// <summary>
    /// 扩展monobehaviour 发送消息方法
    /// </summary>
    /// <param name="mono"></param>
    /// <param name="type"></param>
    /// <param name="area"></param>
    /// <param name="command"></param>
    /// <param name="message"></param>
    public static void WriteMessage(this MonoBehaviour mono, int id, object message) {
        switch (Type) {
            case NetType.TCP:
                TcpClient.I.Write(id, message);
                break;
            case NetType.UDP:
                UdpClient.I.Write(id, message);
                break;
            case NetType.KCP:
                KcpClient.I.Write(id, message);
                break;
        }
    }
    public static void Close(this MonoBehaviour mono) {
        switch (Type) {
            case NetType.TCP:
                TcpClient.I.Close();
                break;
            case NetType.UDP:
                UdpClient.I.Close();
                break;
            case NetType.KCP:
                KcpClient.I.Close();
                break;
        }
    }
    public static void Initial(this MonoBehaviour mono) {
        switch (Type) {
            case NetType.TCP:
                TcpClient.I.ConnectServer();
                break;
            case NetType.UDP:
                UdpClient.I.Initial();
                    break;
            case NetType.KCP:
                KcpClient.I.Initial();
                break;
        }
    }
    public static List<MessageModel> Messages(this MonoBehaviour mono) {
        if (Type == NetType.TCP) {
            return TcpClient.I.messages;
        } else if (Type == NetType.UDP) {
            return UdpClient.I.messages;
        } else if (Type == NetType.KCP) {
            return KcpClient.I.messages;
        }
        return null;
    }
}

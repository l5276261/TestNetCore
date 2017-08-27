using System.Collections;
using System.Collections.Generic;
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
    public static void WriteMessage(this MonoBehaviour mono, byte type, int area, int command, object message) {
        switch (Type) {
            case NetType.TCP:
                NetIO.I.Write(type, area, command, message);
                break;
            case NetType.UDP:
                Udp.I.Write(type, area, command, message);
                break;
            case NetType.KCP:
                KCPSocket.I.Write(type, area, command, message);
                break;
        }
    }
    public static void Close(this MonoBehaviour mono) {
        switch (Type) {
            case NetType.TCP:
                NetIO.I.Close();
                break;
            case NetType.UDP:
                Udp.I.Close();
                break;
            case NetType.KCP:
                KCPSocket.I.Close();
                break;
        }
    }
    public static void Initial(this MonoBehaviour mono) {
        switch (Type) {
            case NetType.TCP:
                NetIO.I.ConnectServer();
                break;
            case NetType.UDP:
                    Udp.I.Initial();
                    break;
            case NetType.KCP:
                KCPSocket.I.Initial();
                break;
        }
    }
    public static List<SocketModel> Messages(this MonoBehaviour mono) {
        if (Type == NetType.TCP) {
            return NetIO.I.messages;
        } else if (Type == NetType.UDP) {
            return Udp.I.messages;
        } else if (Type == NetType.KCP) {
            return KCPSocket.I.messages;
        }
        return null;
    }
}

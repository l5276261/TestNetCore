using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ex {
    public static bool IsUdp;
    /// <summary>
    /// 扩展monobehaviour 发送消息方法
    /// </summary>
    /// <param name="mono"></param>
    /// <param name="type"></param>
    /// <param name="area"></param>
    /// <param name="command"></param>
    /// <param name="message"></param>
    public static void WriteMessage(this MonoBehaviour mono, byte type, int area, int command, object message) {
        if (!IsUdp)
            NetIO.I.Write(type, area, command, message);
        else Udp.I.Write(type, area, command, message);
    }
    public static void Close(this MonoBehaviour mono) {
        if (!IsUdp) NetIO.I.Close();
        else Udp.I.Close();
    }
    public static void Initial(this MonoBehaviour mono) {
        if (!IsUdp) NetIO.I.ConnectServer();
        else Udp.I.Initial();
    }
    public static List<SocketModel> Messages(this MonoBehaviour mono) {
        if (!IsUdp) return NetIO.I.messages;
        else return Udp.I.messages;
    }
}

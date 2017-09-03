using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageModel {
    /// <summary>协议ID </summary>
    public int ID { get; set; }
    /// <summary>消息体，当前需要处理的主体数据 </summary>
    public object Message { get; set; }
    public MessageModel() { }
    public MessageModel(int id, object message) {
        ID = id;
        Message = message;
    }
    public T GetMessage<T>() {
        return (T)Message;
    }
}

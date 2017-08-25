using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.auto
{
    public class SocketModel
    {
        /// <summary>一级协议，用于区分所属模块 </summary>
        public byte Type { get; set; }
        /// <summary>二级协议，用于区分模块下所属子模块 </summary>
        public int Area { get; set; }
        /// <summary>三级协议，用于区分当前处理逻辑功能 </summary>
        public int Command { get; set; }
        /// <summary>消息体，当前需要处理的主体数据 </summary>
        public object Message { get; set; }
        public SocketModel() { }
        public SocketModel(byte type,int area,int command,object message) {
            Type = type;
            Area = area;
            Command = command;
            Message = message;
        }
        public T GetMessage<T>() {
            return (T)Message;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageEncoding:BaseEncode {
    /// <summary>
    /// 消息体序列化，此时传入的model.Message是byte[]转成的object类型，由外部选择消息体的具体序列化方式
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] Encode(object value) {
        MessageModel model = value as MessageModel;
        byte[] m = model.Message == null ? null : model.Message as byte[];
        //byte占位一个字节，int占位4个字节
        int basel = 4;
        int l = basel + (m == null ? 0 : m.Length);
        byte[] r = new byte[l];
        EncodeInt(r, 0, model.ID);
        Buffer.BlockCopy(m, 0, r, basel, m.Length);
        return r;
        #region 性能不好放弃
        //SocketModel model = value as SocketModel;
        //ByteArray ba = new ByteArray();
        //ba.write(model.Type);
        //ba.write(model.Area);
        //ba.write(model.Command);
        ////判断消息体是否为空，不为空则直接写入
        //if (model.Message != null)
        //    //ba.write(SerializeUtil.Encode(model.Message));
        //    ba.write(model.Message as byte[]);
        //byte[] result = ba.getBuff();
        //ba.Close();
        //return result;
        #endregion
    }
    /// <summary>
    /// 消息体反序列化，此时传出的model.Message是byte[]转成的object类型，由外部选择消息体的具体序列化方式
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object Decode(byte[] value) {
        MessageModel model = new MessageModel();
        model.ID = DecodeByte(value, 0);
        if (value.Length > 4) {
            byte[] message = new byte[value.Length - 4];
            Buffer.BlockCopy(value, 4, message, 0, message.Length);
            model.Message = message;
        }
        return model;
        #region 性能不好放弃
        //ByteArray ba = new ByteArray(value);
        //SocketModel model = new SocketModel();
        //byte type;
        //int area;
        //int command;
        ////从数据中读取三层协议，读取数据顺序必须和写入顺序保持一致
        //ba.read(out type);
        //ba.read(out area);
        //ba.read(out command);
        //model.Type = type;
        //model.Area = area;
        //model.Command = command;
        ////判断读取完协议后，是否还有数据需要读取，是则说明有消息体，进行消息体读取
        //if (ba.Readable) {
        //    byte[] message;
        //    //将剩余数据全部读取出来
        //    ba.read(out message, ba.Length - ba.Position);
        //    //剩余数据赋值为消息体
        //    model.Message = message;
        //}
        //ba.Close();
        //return model;
        #endregion
    }
}

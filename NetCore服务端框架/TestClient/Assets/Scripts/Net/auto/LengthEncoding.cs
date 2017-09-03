using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LengthEncoding :BaseEncode{
    /// <summary>粘包长度编码 </summary>
    public static byte[] Encode(byte[] buff) {
        byte[] result = new byte[4 + buff.Length];
        Buffer.BlockCopy(buff, 0, result, 4, buff.Length);
        EncodeInt(result,0, buff.Length);
        return result;
        #region 性能不好放弃
        //MemoryStream ms = new MemoryStream();//创建内存流对象
        //BinaryWriter sw = new BinaryWriter(ms);//写入二进制对象流
        ////写入消息长度
        //sw.Write(buff.Length);
        ////写入消息体
        //sw.Write(buff);
        //byte[] result = new byte[ms.Length];
        //Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
        //sw.Close();
        //ms.Close();
        //return result;
        #endregion
    }
    /// <summary>粘包长度解码 </summary>
    public static byte[] Decode(ref List<byte> cache) {
        if (cache.Count < 4) return null;
        byte[] b = cache.ToArray();
        int l = DecodeInt(b,0);
        //如果消息体长度大于缓存中数据长度，说明消息没有读取完，等待下次消息到达后再次处理
        if (l > b.Length) return null;
        //读取正确长度的数据
        byte[] r = new byte[l];
        Buffer.BlockCopy(b, 4, r, 0, l);
        //清空缓存
        cache.Clear();
        //将读取后剩余的数据写入缓存
        byte[] b1 = new byte[b.Length - 4 - l];
        Buffer.BlockCopy(b, 4 + l, b1, 0, b1.Length);
        cache.AddRange(b1);
        return r;
        #region 性能不好放弃
        //if (cache.Count < 4) return null;
        //MemoryStream ms = new MemoryStream(cache.ToArray());//创建内存流对象，并将缓存数据写入进去
        //BinaryReader br = new BinaryReader(ms);//二进制读取流
        //int length = br.ReadInt32();//从缓存中读取int型消息体长度
        ////如果消息体长度大于缓存中数据长度，说明消息没有读取完，等待下次消息到达后再次处理
        //if (length > ms.Length - ms.Position) {
        //    return null;
        //}
        ////读取正确长度的数据
        //byte[] result = br.ReadBytes(length);
        ////清空缓存
        //cache.Clear();
        ////将读取后剩余的数据写入缓存
        //cache.AddRange(br.ReadBytes((int)(ms.Length - ms.Position)));
        //br.Close();
        //ms.Close();
        //return result;
        #endregion
    }
}

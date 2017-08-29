using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEncode {

    /// <summary>byte编码成字节数组的指定的1位元素 </summary>
    public static void EncodeByte(byte[] bytes, int offset, byte b) {
        bytes[0 + offset] = (byte)b;
    }
    /// <summary>字节数组的指定的1位元素解码成byte </summary>
    public static byte DecodeByte(byte[] b, int offset) {
        return b[0 + offset];
    }
    /// <summary>int编码成字节数组的指定的4位元素 </summary>
    public static void EncodeInt(byte[] b, int offset, int i) {
        b[0 + offset] = (byte)i;
        b[1 + offset] = (byte)(i >> 8);
        b[2 + offset] = (byte)(i >> 16);
        b[3 + offset] = (byte)(i >> 24);
    }
    /// <summary>字节数组的指定的4位元素解码成int </summary>
    public static int DecodeInt(byte[] b, int offset) {
        //不知道后面两个方法哪个好
        #region 方法一
        //int i = 0;
        //i |= (int)b[0 + offset];
        //i |= (int)(b[1 + offset] << 8);
        //i |= (int)(b[2 + offset] << 16);
        //i |= (int)(b[3 + offset] << 24);
        //return i;
        #endregion
        //方法二
        return BitConverter.ToInt32(b, offset);
    }
    /// <summary>uint32编码成字节数组的指定的4位元素 </summary>
    public static void EncodeUInt(byte[] b, int offset, UInt32 i) {
        b[0 + offset] = (byte)i;
        b[1 + offset] = (byte)(i >> 8);
        b[2 + offset] = (byte)(i >> 16);
        b[3 + offset] = (byte)(i >> 24);
    }
    /// <summary>字节数组的指定的4位元素解码成uint32 </summary>
    public static UInt32 DecodeUInt(byte[] b, int offset) {
        //不知道后面两个方法哪个好
        #region 方法一
        //UInt32 i = 0;
        //i |= (UInt32)b[0+ offset];
        //i |= (UInt32)(b[1+ offset] << 8);
        //i |= (UInt32)(b[2+ offset] << 16);
        //i |= (UInt32)(b[3+ offset] << 24);
        //return i;
        #endregion
        //方法二
        return BitConverter.ToUInt32(b, offset);
    }
}

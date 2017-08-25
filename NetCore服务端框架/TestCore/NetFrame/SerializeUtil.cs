using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NetFrame
{
    public class SerializeUtil
    {
        /// <summary>使用PB还是JSON序列化方式对消息体的message进行序列化操作 </summary>
        public static bool IsPBOrJson;
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static byte[] SerEncode<T>(T t) {
            if (IsPBOrJson) return PBSer<T>(t);
            else return JsonEncode(t);
        }
        /// <summary>
        /// 对象反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DesDecode<T>(byte[] value) {
            if (IsPBOrJson) return PBDes<T>(value);
            else return JsonDecode<T>(value);
        }
        /// <summary>PB序列化操作 </summary>
        public static byte[] PBSer<T>(T t) {
            MemoryStream ms = new MemoryStream();
            //BinaryFormatter bm = new BinaryFormatter();
            //bm.Serialize(ms, p);
            Serializer.Serialize<T>(ms, t);
            byte[] data = ms.ToArray();//length=27  709
            return data;
        }
        /// <summary>PB反序列化操作 </summary>
        public static T PBDes<T>(byte[] data) {
            MemoryStream ms = new MemoryStream(data);
            // BinaryFormatter bm1 = new BinaryFormatter();
            //Person p1= bm.Deserialize(ms1) as Person;
            T t = Serializer.Deserialize<T>(ms);
            return t;
        }
        /// <summary>
        /// Json序列化对象为二进制数组
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] JsonEncode(object value) {
            return StringToBytes(JsonSer(value));
        }
        /// <summary>
        /// Json反序列化二进制数组为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T JsonDecode<T>(byte[] bytes) {
            return JsonDes<T>(BytesToString(bytes));
        }
        /// <summary>字符串转字节数组（二进制数组） </summary>
        public static byte[] StringToBytes(string str) {
            return Encoding.Default.GetBytes(str);
        }
        /// <summary>字节数组（二进制数组）转字符串 </summary>
        public static string BytesToString(byte[] bytes) {
            return Encoding.Default.GetString(bytes);
        }
        /// <summary>JSON序列化操作 </summary>
        public static string JsonSer(object value) {
            return JsonConvert.SerializeObject(value);
        }
        /// <summary>JSON反序列化操作 </summary>
        public static T JsonDes<T>(string str) {
            return JsonConvert.DeserializeObject<T>(str);
        }
        #region BinaryFormatter对象序列化方式无法和unity客户端正常发送非基本类型数据，放弃使用
        /// <summary>
        /// BinaryFormatter对象序列化，core平台的序列化非基本类型的时候结果跟unity的不一致，导致双方解析失败，
        /// 基本类型的即使结果不同也一样可以解析
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Encode(object value) {
            MemoryStream ms = new MemoryStream();//创建编码解码的内存流对象
            BinaryFormatter bw = new BinaryFormatter();//二进制流序列化对象
            //将obj对象序列化成二进制数据，写入到内存流
            bw.Serialize(ms, value);
            byte[] result = new byte[ms.Length];
            //将流数据 拷贝到结果数组
            Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
            ms.Close();
            return result;
        }
        /// <summary>
        /// BinaryFormatter反序列化对象，core平台的序列化非基本类型的时候结果跟unity的不一致，导致双方解析失败，
        /// 基本类型的即使结果不同也一样可以解析
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object Decode(byte[] value) {
            MemoryStream ms = new MemoryStream(value);//创建编码解码的内存流对象，并将需要反序列化的数据写入其中
            BinaryFormatter bw = new BinaryFormatter();//二进制流序列化对象
            //将流数据反序列化为obj对象
            object result = bw.Deserialize(ms);
            ms.Close();
            return result;
        }
        #endregion
    }
}

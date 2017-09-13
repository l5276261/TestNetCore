using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataModule
{
    public static class DataMethods{
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
        /// <summary>JSON序列化操作 </summary>
        public static string JsonSer(object value) {
            return JsonConvert.SerializeObject(value);
        }
        /// <summary>JSON反序列化操作 </summary>
        public static T JsonDes<T>(string str) {
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}

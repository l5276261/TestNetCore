using GameProtocol.dto;
using NetFrame;
using NetFrame.auto;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TestCore.LOLServer;
using TestCore.Model;
using TestCore.Tool;

namespace TestCore{
    [ProtoContract]
    public class TestPB {
        [ProtoMember(1)]
        public byte A;
        [ProtoMember(2)]
        public int B;
        [ProtoMember(3)]
        public int C;
        [ProtoMember(4)]
        public string D;
    }
    [Serializable]
    public struct SS {
        public int Age;
        public string Name;
    }
    [Serializable]
    public enum EE {
        A,B,C
    }
    public static class Test{
        public static void PB() {
            Console.WriteLine("普通数据PB测试");
            List<string> lstr = new List<string>() { "a"};
            Console.WriteLine("原始的List<string>数据 " + lstr[0]);
            byte[] b = Methods.PBSer(lstr);
            lstr = Methods.PBDes<List<string>>(b);
            Console.WriteLine("经PB序列化后反序列化的List<string>数据 " + lstr[0]);
            PBData data = new PBData() {
                Name = "a", Age = 20, IsBoy = true, Data = new PBData() {
                    Name = "b", Age = 30, IsBoy = false,
                }
            };
            Console.WriteLine("原始的PBData数据 " + data.Name+" "+data.Age+" "+data.IsBoy+" "+
                data.Data.Name+" "+data.Data.Age+" "+data.Data.IsBoy);
            b = Methods.PBSer(data);
            data = Methods.PBDes<PBData>(b);
            Console.WriteLine("经PB序列化后反序列化的PBData数据 " + data.Name + " " + data.Age + " " + data.IsBoy + " " + 
                data.Data.Name + " " + data.Data.Age + " " + data.Data.IsBoy);
            int i = 1;
            Console.WriteLine("原始的基本类型int " + i);
            b = Methods.PBSer(i);
            i = Methods.PBDes<int>(b);
            Console.WriteLine("经PB序列化后反序列化的基本类型int " + i);
            string str = "aaa";
            Console.WriteLine("原始的基本类型string " + str);
            b = Methods.PBSer(str);
            str = Methods.PBDes<string>(b);
            Console.WriteLine("经PB序列化后反序列化的基本类型string " + str);
        }
        public static void JSON() {
            List<string> lstr = new List<string>() { "a" };
            Console.WriteLine("原始的List<string>数据 " + lstr[0]);
            string s = Methods.JsonSer(lstr);
            Console.WriteLine("经JSON序列化后的List<string>数据 " + s);
            lstr = Methods.JsonDes<List<string>>(s);
            Console.WriteLine("经JSON序列化后反序列化的List<string>数据 " + lstr[0]);
            PBData data = new PBData() {
                Name = "a", Age = 20, IsBoy = true, Data = new PBData() {
                    Name = "b", Age = 30, IsBoy = false,
                }
            };
            Console.WriteLine("原始的PBData数据 " + data.Name + " " + data.Age + " " + data.IsBoy + " " +
              data.Data.Name + " " + data.Data.Age + " " + data.Data.IsBoy);
            s = Methods.JsonSer(data);
            Console.WriteLine("经JSON序列化后的PBData数据 " + s);
            data = Methods.JsonDes<PBData>(s);
            Console.WriteLine("经JSON序列化后反序列化的PBData数据 " + data.Name + " " + data.Age + " " + data.IsBoy + " " +
    data.Data.Name + " " + data.Data.Age + " " + data.Data.IsBoy);
            int i = 1;
            s = Methods.JsonSer(i);
            Console.WriteLine("经JSON序列化后的基本类型int " + s);
            i = Methods.JsonDes<int>(s);
            Console.WriteLine("经JSON序列化后反序列化的基本类型int "+i);
            string str = "aaa";
            s = Methods.JsonSer(str);
            Console.WriteLine("经JSON序列化后的基本类型string " + s);
            str = Methods.JsonDes<string>(s);
            Console.WriteLine("经JSON序列化后反序列化的基本类型string " + str);
        }
        public static void PB_JSON_BytesLength() {
            List<string> lstr = new List<string>() { "a" };
            byte[] b = Methods.PBSer(lstr);
            Console.WriteLine("经PB序列化后的List<string>数据的字节数组长度 " + b.Length);
            b = SerializeUtil.JsonEncode(lstr);
            Console.WriteLine("经JSON序列化后的List<string>数据的字节数组长度 " + b.Length);
            PBData data = new PBData() {
                Name = "a", Age = 20, IsBoy = true, Data = new PBData() {
                    Name = "b", Age = 30, IsBoy = false,
                }
            };
            b = Methods.PBSer(data);
            Console.WriteLine("经PB序列化后反序列化的PBData数据的字节数组长度 " + b.Length);
            b = SerializeUtil.JsonEncode(data);
            Console.WriteLine("经JSON序列化后的PBData数据的字节数组长度 " + b.Length);
            int i = 9;
            b = Methods.PBSer(i);
            Console.WriteLine("经PB序列化后反序列化的基本类型int " + i + "的字节数组长度 " + b.Length);
            b = SerializeUtil.JsonEncode(i);
            Console.WriteLine("经JSON序列化后反序列化的基本类型int " + i + "的字节数组长度 " + b.Length);
            string str = "aaa";
            b = Methods.PBSer(str);
            Console.WriteLine("经PB序列化后反序列化的基本类型string " + str + "的字节数组长度 " + b.Length);
            b = SerializeUtil.JsonEncode(str);
            Console.WriteLine("经JSON序列化后反序列化的基本类型string " + str + "的字节数组长度 " + b.Length);
            bool isTrue = true;
            b = Methods.PBSer(isTrue);
            Console.WriteLine("经PB序列化后反序列化的基本类型int " + isTrue + "的字节数组长度 " + b.Length);
            b = SerializeUtil.JsonEncode(isTrue);
            Console.WriteLine("经JSON序列化后反序列化的基本类型int " + isTrue + "的字节数组长度 " + b.Length);
        }
        public static void Path() {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            path = Methods.ChangeAppPath(path);
            path += @"Json\" + "a" + ".json";
            path = Methods.LinuxPath(path);
            //path = Directory.GetCurrentDirectory();
            Console.WriteLine(path);
        }
        public static void WriteJson() {
            PBData data = new PBData() { Name = "名字", Age = 13, IsBoy = true };
            string str = Methods.JsonSer(data);
            Methods.WriteJson("PBData", str);
        }
        public static void ReadJson() {
            PBData data = Methods.ReadJson<PBData>("PBData");
            Console.WriteLine(data.Name + " " + data.Age + " " + data.IsBoy);
        }
        public static void Config() {
            Methods.Config();
        }
        public static void Int_Bytes(int num) {
            byte[] b = new byte[4];
            LengthEncoding.EncodeInt(b,0,num);
            Console.WriteLine("num " + num + " 的二进制格式为 " + Convert.ToString(num, 2));
            for (int i = 0; i < 4; i++) {
                Console.WriteLine("b[i] " + b[i] + " 的二进制格式为 " + Convert.ToString(b[i], 2));
            }
            num = LengthEncoding.DecodeInt(b,0);
            Console.WriteLine("通过二进制把byte数组再转换成int " + num + " 对应的二进制为 " + Convert.ToString(num, 2));
        }
        public static void LittleThing() {
            byte[] b = Encoding.UTF8.GetBytes("\r\n\r\n");
            string s = "";
            for (int j = 0; j < b.Length; j++) {
                s += j;
            }
            Console.WriteLine(s);
        }
        public static void Normal_JSON_PB_Bytes() {
            TestPB t = new TestPB() { A = 1, B = 1000, C = 10000, D = "jajdfaijfikafjaksjfskf" };
            byte[]b = SerializeUtil.StringToBytes(t.D);
            Console.WriteLine("普通长度 " + (b.Length + 4 + 1 + 4 + 4));//string本身转换bytes的长度，加长度标记，还有其他byte，int，int的长度
            b = SerializeUtil.JsonEncode(t);
            Console.WriteLine("JSON后长度 " + (b.Length+4));
            b = SerializeUtil.PBSer(t);
            Console.WriteLine("PB后长度 " + (b.Length + 4));
        }
        public static void HandlerDic() {
            //HandlerCenter h = new HandlerCenter();
            //var l = h.handlerDic[1];
            //var l1 = l.Clone();
            //l.Set("我是前任");l.Do();
            //l1.Set("我是现任");l1.Do();l.Do();
        }
        public static void Mysql() {
            //Methods.MysqlFind();
        }
        public static void NetFrameStart() {
            //服务器初始化
            ServerStart ss = new ServerStart(9000);
            ss.Encode = MessageEncoding.Encode;
            ss.Decode = MessageEncoding.Decode;
            ss.Center = new HandlerCenter();
            ss.LD = LengthEncoding.Decode;
            ss.LE = LengthEncoding.Encode;
            ss.Start(6666);
            SerializeUtil.IsPBOrJson = false;
            UserToken.Type = NetType.TCP;
            Console.WriteLine("TCP服务器启动成功");
            while (true) {

            }
        }
        public static void NetFrameUdpStart() {
            //服务器初始化
            UdpServer ss = new UdpServer(3);
            ss.Encode = MessageEncoding.Encode;
            ss.Decode = MessageEncoding.Decode;
            ss.Center = new HandlerCenter();
            ss.LD = LengthEncoding.Decode;
            ss.LE = LengthEncoding.Encode;
            ss.Start(6666);
            SerializeUtil.IsPBOrJson = false;
            UserToken.Type = NetType.UDP;
            Console.WriteLine("UDP服务器启动成功");
            while (true) {

            }
        }
        public static void NetFrameKcpStart() {
            //服务器初始化
            KcpServer ss = new KcpServer(3);
            ss.Encode = MessageEncoding.Encode;
            ss.Decode = MessageEncoding.Decode;
            ss.Center = new HandlerCenter();
            ss.LD = LengthEncoding.Decode;
            ss.LE = LengthEncoding.Encode;
            ss.Start(6666);
            SerializeUtil.IsPBOrJson = false;
            UserToken.Type = NetType.KCP;
            Console.WriteLine("KCP服务器启动成功");
            Thread t = new Thread(delegate () {
                while (true) {
                    if (TokenManager.TokenDic.ContainsKey(1))
                        TokenManager.TokenDic[1].Update();
                    //Thread.Sleep(1);
                }
            }) { IsBackground = true };
            t.Start();
            while (true) {

            }
        }
    }
}

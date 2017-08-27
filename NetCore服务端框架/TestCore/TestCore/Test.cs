using GameProtocol.dto;
using NetFrame;
using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TestCore.LOLServer;
using TestCore.Model;
using TestCore.Tool;

namespace TestCore{
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
            UserToken.IsUdp = false;
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
            UserToken.IsUdp = true;
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
            //UserToken.IsUdp = true;
            Console.WriteLine("KCP服务器启动成功");
            Thread t = new Thread(delegate () {
                while (true) {
                    if (TokenManager.TokenDic.ContainsKey(1))
                        TokenManager.TokenDic[1].Update();
                    Thread.Sleep(10);
                }
            });
            t.Start();
            while (true) {

            }
        }
    }
}

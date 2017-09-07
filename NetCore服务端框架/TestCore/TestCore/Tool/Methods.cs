using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Reflection;
using TestCore.Server.Logic.Login;
using TestCore.Server.Logic;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
//using MySql.Data.MySqlClient;

namespace TestCore.Tool {
    public static class Methods {
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
        /// <summary>判断是不是Linux系统 </summary>
        public static bool IsUnix() {
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }
        #region 忽略
        /// <summary>
        /// 由于core的AppDomain.CurrentDomain.BaseDirectory是bin\Debug\netcoreapp2.0\（Windows写法），不方便，
        /// 所以修改一下路径为bin的上一级目录
        /// 参数为BaseDirectory的值，返回bin的上一级目录
        /// 不用此方法，发现Directory.GetCurrentDirectory()可以直接得到bin的上一级目录，不过不是\或者/结尾
        /// </summary>
        public static string ChangeAppPath(string path) {
            if (IsUnix()) {
                return path.Replace(@"bin/Debug/netcoreapp2.0/", "");
            } else {
                return path.Replace(@"bin\Debug\netcoreapp2.0\", "");
            }
        }
        #endregion
        /// <summary>Windows和Linux路径的写法不一样，前者\后者/，此方法用来变成Linux的写法，奇怪的是Linux的写法在Windows也是正常的 </summary>
        public static string LinuxPath(string path) {
            return path.Replace(@"\",@"/");
        }
        /// <summary>读取bin上一级目录下Json文件夹下的json格式文件 </summary>
        public static T ReadJson<T>(string fileName) {
            //string path = ChangeAppPath(AppDomain.CurrentDomain.BaseDirectory);
            string path = Directory.GetCurrentDirectory() + @"\";
            path += @"Json\" + fileName + ".json";//fileName后面可以是.json或者.Json，比如组合成"Card.json"或者"Card.Json"
            path = LinuxPath(path);//不需要判断是不是Linux，因为/测试的在Windows也是正常的
            if (!File.Exists(path)) return default(T);
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            string curstr = sr.ReadToEnd();
            if (curstr == null || curstr.Length == 0) return default(T);
            T t = JsonConvert.DeserializeObject<T>(curstr);
            sr.Close();
            return t;
        }
        /// <summary>生成bin上一级目录下Json文件夹下的json格式文件 </summary>
        public static void WriteJson(string fileName, string str) {
            //string path = ChangeAppPath(AppDomain.CurrentDomain.BaseDirectory);
            string path = Directory.GetCurrentDirectory() + @"\";
            path += @"Json\" + fileName + ".json";
            path = LinuxPath(path);//不需要判断是不是Linux，因为/测试的在Windows也是正常的
            FileStream fs2 = new FileStream(path, FileMode.Create, FileAccess.Write);//创建写入文件 
            StreamWriter sw2 = new StreamWriter(fs2);
            sw2.WriteLine(str);//开始写入值
            sw2.Close();
            fs2.Close();
        }
        /// <summary>读取配置文件json别忘了添加包
        /// Microsoft.Extensions.Configuration和Microsoft.Extensions.Configuration.Json
        /// Configuration相关的包会直接整合到项目中，所以到了Linux平台不需要重新下载该包
        /// </summary>
        public static void Config() {
            var builder0 = new ConfigurationBuilder();
            builder0.AddInMemoryCollection();           
            var config = builder0.Build();
            config["a"] = "a12";
            Console.WriteLine(config["a"]);

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var Configuration = builder.Build();
            Console.WriteLine($"option1 = {Configuration["option1"]}");
            Console.WriteLine($"option2 = {Configuration["option2"]}");
            Console.WriteLine(
                $"suboption1 = {Configuration["subsection:suboption1"]}");
            Console.WriteLine();

            Console.WriteLine("Wizards:");
            Console.Write($"{Configuration["wizards:0:Name"]}, ");
            Console.WriteLine($"age {Configuration["wizards:0:Age"]}");
            Console.Write($"{Configuration["wizards:1:Name"]}, ");
            Console.WriteLine($"age {Configuration["wizards:1:Age"]}");
        }
        /// <summary>字符串转字节数组（二进制数组） </summary>
        public static byte[] StringToBytes(string str) {
            return Encoding.Default.GetBytes(str);
        }
        /// <summary>字节数组（二进制数组）转字符串 </summary>
        public static string BytesToString(byte[]bytes) {
            return Encoding.Default.GetString(bytes);
        }
        public static IHandler Assembly_CreateInstance(string path,string name) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            object obj = assembly.CreateInstance(path+ name);
            return (IHandler)obj;
        }
        public static IConfigurationRoot GetConfig(string path) {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(path);
            var config = builder.Build();
            return config;
        }
        public static Dictionary<int, string> ConfigToDic(IConfigurationRoot config) {
            Dictionary<int, string> d = new Dictionary<int, string>();
            int key;
            foreach (var pair in config.AsEnumerable()) {
                if (int.TryParse(pair.Key, out key))
                    d.Add(key, pair.Value);
            }
            return d;
        }
        public static Dictionary<int, IHandler> GetIHandlerDic(string path,Dictionary<int, string> nameDic) {
            Dictionary<int, IHandler> d = new Dictionary<int, IHandler>();
            foreach (var key in nameDic.Keys) {
                d[key] = Methods.Assembly_CreateInstance(path, nameDic[key]);
            }
            return d;
        }
        public static bool Interlocked_Compare(ref int i) {
            int initial, computed = 0;bool res;
            do {
                initial = i;
                if (initial > 0) {
                    computed = initial - 1;
                    res = true;
                } else {
                    res = false;break;
                }
            } while (initial != Interlocked.CompareExchange(
      ref i, computed, initial));
            return res;
        }
        public static void Interlocked_Num(ref int i) {
                int sum = 0;
                for (int j = 0; j < 10; j++) {
                //用Interlocked的方法是多线程安全的，数字增加或者减少同时返回计算后结果，原子操作
                //sum = Interlocked.Increment(ref i);
                sum = Interlocked.Add(ref i, 1);
                //这个不是多线程安全的，不能保证数据一致性，可能会导致返回的值不是此时此刻计算后的i，可是我用几个多线程测试未见异常
                //sum = ++i;
                Console.Write(" sum "+sum+" i "+i+" ");
            }
            Console.WriteLine("");
        }
        public static void ThreadNum() {
            int minCLR, minIO, maxCLR, maxIO, avaiCLR, avaiIO;
            ThreadPool.GetMinThreads(out minCLR, out minIO);
            ThreadPool.GetMaxThreads(out maxCLR, out maxIO);
            ThreadPool.GetAvailableThreads(out avaiCLR, out avaiIO);
            Console.WriteLine(minCLR + " " + minIO + " " + maxCLR + " " + maxIO + " " + avaiCLR + " " + avaiIO);
            //ThreadPool.SetMinThreads(4, 4);
            //ThreadPool.SetMaxThreads(4, 4);
            for (int i = 0; i < 4; i++) {
                int j = i;
                Task.Run(delegate () {
                    ThreadPool.GetAvailableThreads(out avaiCLR, out avaiIO);
                    Console.WriteLine(minCLR + " " + minIO + " " + maxCLR + " " + maxIO + " " + avaiCLR + " " + avaiIO);
                    while (true) {
                        Thread.Sleep(5000);
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                    }
                });
            }
            //ThreadPool.GetMinThreads(out minCLR, out minIO);
            //ThreadPool.GetMaxThreads(out maxCLR, out maxIO);
            //ThreadPool.GetAvailableThreads(out avaiCLR, out avaiIO);
            //Console.WriteLine(minCLR + " " + minIO + " " + maxCLR + " " + maxIO + " " + avaiCLR + " " + avaiIO);
            while (true) {

            }
        }
        public static void SetThreadNum(int workerMax,int ioMax, int workerMin=0, int ioMin=0) {
            int defaultWorkerMin, defaultIOMin;
            ThreadPool.GetMinThreads(out defaultWorkerMin, out defaultIOMin);
            if (workerMin == 0) workerMin = defaultWorkerMin / 2-1;
            if (ioMin == 0) ioMin = defaultIOMin / 2-1;
            ThreadPool.SetMinThreads(workerMin, ioMin);
            ThreadPool.SetMaxThreads(workerMax, ioMax);
        }
        //注意task开启是默认是后台线程，如果主线程不一直开启，会自动结束前者。
        public static void TaskRun(Action a) {
            Task.Run(delegate () {
                a();
            });
            //让主线程一直运行，以保证后台线程可以一直运行
            while (true) {

            }
        }
        /// <summary>
        /// StackExchange.Redis和ServiceStack.Redis都是NuGet包更新后可以直接其他平台或者机子使用类似Configuration的包。
        /// 不过最好都改成使用拷贝的包里面的dll的形式，方便其他项目复用。不过Configuration相关比较多，暂时还是使用的NuGet的包
        /// </summary>
        public static void RedisConnect() {
            //RedisClient redisCli = new RedisClient("127.0.0.1", 6379);
            ConnectionMultiplexer connect = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            var db = connect.GetDatabase();
            db.StringSet("a","A");
            var s = db.StringGet("a");
            Console.WriteLine(s);
        }
        #region 测试Mysql，目前的正式版不支持core2.0，只有预览版
        //public static void MysqlFind() {
        //    try {
        //        MySqlConnection con = new MySqlConnection("server=127.0.0.1;database=snscenter;uid=root;pwd=123456;charset='gbk';SslMode=None");
        //        con.Open();
        //        bool result = con.Ping();
        //        Console.WriteLine(result);
        //    } catch (Exception e) {
        //        Console.WriteLine("连接失败 " + e.Message);
        //    }
        //    Console.WriteLine("连接成功");
        //    //新增数据
        //}
        #endregion
    }
}

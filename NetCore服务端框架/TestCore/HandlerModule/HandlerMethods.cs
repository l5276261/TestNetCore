using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HandlerModule
{
    public static class HandlerMethods{
        public static IHandler Assembly_CreateInstance(string path, string name) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            object obj = assembly.CreateInstance(path + name);
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
        public static Dictionary<int, IHandler> GetIHandlerDic(string path, Dictionary<int, string> nameDic) {
            Dictionary<int, IHandler> d = new Dictionary<int, IHandler>();
            foreach (var key in nameDic.Keys) {
                d[key] = Assembly_CreateInstance(path, nameDic[key]);
            }
            return d;
        }
    }
}

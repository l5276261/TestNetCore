using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class Methods {
    /// <summary>从Resources读取actionsettings配置 </summary>
    public static Dictionary<int, string> LoadProtocol(string fileNmae) {
        TextAsset txt = Resources.Load<TextAsset>(fileNmae);
        Dictionary<int, string> d = JsonConvert.DeserializeObject<Dictionary<int, string>>(txt.text);
        string[] s = null;
        Dictionary<int, string> d1 = new Dictionary<int, string>(); 
        foreach (var key in d.Keys) {
            s = d[key].Split('.');
            d1[key] = s[s.Length - 1];
        }
        return d1;
    }
    public static IHandler Assembly_CreateInstance(string name) {
        Assembly assembly = Assembly.GetExecutingAssembly();
        object obj = assembly.CreateInstance(name);
        if (obj == null) Debug.Log("空object");
        return (IHandler)obj;
    }
    public static Dictionary<int, IHandler> GetIHandlerDic(Dictionary<int, string> nameDic) {
        Dictionary<int, IHandler> d = new Dictionary<int, IHandler>();
        foreach (var key in nameDic.Keys) {
            d[key] = Assembly_CreateInstance(nameDic[key]);
        }
        return d;
    }
}

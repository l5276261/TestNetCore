using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>ping非常不准，放弃使用，直接用收发数据包的时间差当做延迟时间 </summary>
public class PingGO : InstanceMono<PingGO> {
    private static string s_ip = "127.0.0.1:6666";//"116.62.229.34:81";//"127.0.0.1:6666";
    private static System.Action<int> s_callback = null;
    private static int s_timeout = 2;
    // Use this for initialization
    void Start () {
        s_callback = Out;
    }
    public void PingTest() {
        switch (Application.internetReachability) {
            case NetworkReachability.ReachableViaCarrierDataNetwork: // 3G/4G
            case NetworkReachability.ReachableViaLocalAreaNetwork: // WIFI
                {
                    StopCoroutine(this.PingConnect());
                    StartCoroutine(this.PingConnect());
                }
                break;
            case NetworkReachability.NotReachable: // 网络不可用
            default: {
                    if (s_callback != null) {
                        s_callback(-1);
                        Destroy(this.gameObject);
                    }
                }
                break;
        }
    }
    /// <summary>
    /// 超时时间（单位秒）
    /// </summary>
    public static int Timeout {
        set {
            if (value > 0) {
                s_timeout = value;
            }
        }
        get { return s_timeout; }
    }

    // Update is called once per frame
    void Update () {

    }
    IEnumerator PingConnect() {
        // Ping網站 
        Ping ping = new Ping(s_ip);

        int addTime = 0;
        int requestCount = s_timeout * 10; // 0.1秒 请求 1 次，所以请求次数是 n秒 x 10

        // 等待请求返回
        while (!ping.isDone) {
            yield return new WaitForSeconds(0.1f);

            // 链接失败
            if (addTime > requestCount) {
                addTime = 0;

                if (s_callback != null) {
                    s_callback(ping.time);
                    ping.DestroyPing();
                    //Destroy(this.gameObject);
                }
                yield break;
            }
            addTime++;
        }

        // 链接成功
        if (ping.isDone) {
            if (s_callback != null) {
                s_callback(ping.time);
                ping.DestroyPing();
                //Destroy(this.gameObject);
            }
            yield return null;
        }
    }
    private void Out(int time) {
        Debug.Log("延迟时间 " + time);
    }
    #region 另一种Ping使用方法，非携程，先
    float delayTime;
    Ping ping;
    //初始化ping，先执行一次这个方法
    public void SendPing() {
        ping = new Ping(s_ip);
    }
    //输出延迟时间，即ping结果，如果Ping执行完，会再次初始化一个继续执行
    public void PingTest1() {
        Out((int)delayTime);
        if (null != ping && ping.isDone) {
            delayTime = ping.time;
            ping.DestroyPing();
            ping = null;
            Invoke("SendPing", 1.0F);//每秒Ping一次
        }
    }
    #endregion
}

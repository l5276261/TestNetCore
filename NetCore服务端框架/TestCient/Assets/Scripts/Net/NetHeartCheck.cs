using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum OffLineState { Register,LoginAccount,LoginZone,CreateRole,InGame }
public class NetHeartCheck :InstanceMono<NetHeartCheck> {
    public List<string> serverIDlist;
    public List<string> serverNameList;
    public int serverIDIndex = 0;
    public bool isTimeOut = false;
    public int LoginTimeOutMilliseconds = 100;
    private float reconnectInterval = 2f;
    public float reconnectIntervalTimer;
    public bool isClientOffLine;
    public OffLineState OffLine;
    private string warning;
    public bool isReconnect;
    void Awake() {
        serverIDlist = new List<string>();
        //serverIDlist.Add("192.168.5.104:9001");
        //serverIDlist.Add("192.168.5.104:9002");
        serverIDlist.Add("192.168.86.1:9001");
        serverIDlist.Add("192.168.86.1:9002");
        //阿里云IP
        //serverIDlist.Add("120.27.92.40:9001");
        //serverIDlist.Add("120.27.92.40:9002");
        reconnectIntervalTimer = reconnectInterval;
}
    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        if (isTimeOut) {
            reconnectIntervalTimer -= Time.deltaTime;
            if (reconnectIntervalTimer <= 0) {
                if (isClientOffLine)
                {
                    print("您网络已断开");
                    isClientOffLine = false;
                    warning = "您网络已断开";
                    //UIMethods.Instance.PanelAndTextAppear(LoadScene.Instance.LoginAgainRect.gameObject, text: LoadScene.Instance.LoginAgainText, content: warning);
                }
                else {
                    print("连接超时");
                    warning = "连接超时";
                    //UIMethods.Instance.PanelAndTextAppear(LoadScene.Instance.LoginAgainRect.gameObject, text: LoadScene.Instance.LoginAgainText, content: warning);
                }
                isTimeOut = false;
                reconnectIntervalTimer = reconnectInterval;

                #region 登录也自动重连的，没必要
                //if (serverIDIndex == 0)
                //{
                //    if (OffLine == OffLineState.LoginAccount)
                //    {
                //        LoadScene.Instance.NetReset();//这个是必须的，不然超时时间长了会出现_socket参数空的异常错误和什么不能主线程调用的错误
                //        LoadScene.Instance.Login();
                //    }
                //    else if (OffLine == OffLineState.Register) {
                //        LoadScene.Instance.NetReset();
                //        LoadScene.Instance.Register();
                //    }
                //}
                //else {
                //    //TODO重新登录区服，让区服绑定session的意思，是重连的重新登录区服，和正常登录不一样。
                //    //TODO重新发送某些业务action
                //}
                #endregion
                NetReset();
                switch (OffLine) {
                    //case OffLineState.Register:
                    //    UIMethods.Instance.PanelAndTextAppear(LoadScene.Instance.LoginAgainRect.gameObject, Text: LoadScene.Instance.LoginAgainText, content: warning);
                    //    break;
                    //case OffLineState.LoginAccount:
                    //    UIMethods.Instance.PanelAndTextAppear(LoadScene.Instance.LoginAgainRect.gameObject, Text: LoadScene.Instance.LoginAgainText, content: warning);
                    //    break;
                    //case OffLineState.LoginZone:
                    //    UIMethods.Instance.PanelAndTextAppear(LoadScene.Instance.LoginAgainRect.gameObject, Text: LoadScene.Instance.LoginAgainText, content: warning);
                    //    break;
                    //case OffLineState.CreateRole:
                    //    //UIMethods.Instance.PanelAndTextAppear(LoadScene.Instance.CreateRoleErrorRect.gameObject, Text: LoadScene.Instance.CreateRoleErrorText, content: warning);
                    //    UIMethods.Instance.PanelAndTextAppear(LoadScene.Instance.CreateRoleErrorRect.gameObject, Text: LoadScene.Instance.CreateRoleErrorText, content: "断网请重新登录");
                    //    StartCoroutine("ReLogin",2f);
                    //    //Reconnect();
                    //    break;
                    case OffLineState.InGame:
                        StartCoroutine("ReLogin",0.5f);
                        //Reconnect();
                        break;
                }
            }
        }
	}
    public void NetReset()
    {
        //if (Net.Instance.mSocket != null)
        //    Net.Instance.mSocket.Close();
        //Net.Instance.mSocket = null;
    }
    private void Reconnect() {
        //print("游戏中断线自动重连中");
        //NetWriter.SetUrl(serverIDlist[serverIDIndex]);
        //ActionParam a = new ActionParam();
        //a["isReconnectStr"] = JsonUtils.Serialize(ProtoBufUtils.Serialize(true));
        //发送角色信息给服务器，同时会建立连接，不用发送完整的信息，只要能建立连接就行
        //Net.Instance.Send((int)ActionType.EnterGameServer, null, a);//必须使用同一个登录区服action来重新连接，即使另一个action的内容是相同的也不行，估计是服务端对第一次登录的action进行的绑定
    }
    IEnumerator ReLogin(float time) {
        NetReset();
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync("00-LoadScene");
    }
}

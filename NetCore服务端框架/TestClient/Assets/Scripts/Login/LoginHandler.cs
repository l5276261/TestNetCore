using GameProtocol;
using GameProtocol.dto;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginHandler : MonoBehaviour, IHandler {
    string str;
    public void MessageReceive(MessageModel model) {
        List<AccountInfoDTO> l = SerializeUtil.DesDecode<List<AccountInfoDTO>>(model.Message as byte[]);
        string s = "";
        for (int i = 0; i < l.Count; i++) {
            s += l[i].account;
        }
        Debug.Log(s);
    }
    /// <summary>
    /// 登陆返回处理
    /// </summary>
    private void login(int value) {
        //SendMessage("openLoginBtn");
        switch (value) {
            case 0:
                // WarrningManager.errors.Add(new WarrningModel(""));
                Debug.Log("登录成功");
                //加载游戏主场景
                //Application.LoadLevel(1);
                break;
            case -1:
                //WarrningManager.errors.Add(new WarrningModel("帐号不存在"));
                Debug.Log("帐号不存在");
                break;
            case -2:
                //WarrningManager.errors.Add(new WarrningModel("帐号在线"));
                Debug.Log("帐号在线");
                break;
            case -3:
                //WarrningManager.errors.Add(new WarrningModel("密码错误"));
                Debug.Log("密码错误");
                break;
            case -4:
                //WarrningManager.errors.Add(new WarrningModel("输入不合法"));
                Debug.Log("输入不合法");
                break;
        }

    }
    /// <summary>
    /// 注册返回处理
    /// </summary>
    private void reg(int value) {
        switch (value) {
            case 0:
                //WarrningManager.errors.Add(new WarrningModel("注册成功"));
                Debug.Log("注册成功");
                //加载游戏主场景
                break;
            case 1:
                //WarrningManager.errors.Add(new WarrningModel("注册失败，帐号重复"));
                Debug.Log("注册失败，帐号重复");
                break;

        }
    }
    public string Get() {
        return "哈哈哈哈";
    }

    public void Set(object value) {
        str = (string)value;
        Debug.Log("得到消息 ");
    }

    public void Do() {
        Debug.Log("得到的消息是 " + str);
    }

    public IHandler Clone() {
        return new LoginHandler();
    }
}

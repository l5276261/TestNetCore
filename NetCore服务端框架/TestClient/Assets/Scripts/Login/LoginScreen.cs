using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour {
    #region 登陆面板部分
    [SerializeField]
    private InputField accountInput;
    [SerializeField]
    private InputField passwordInput;
    #endregion
    #region 注册面板部分
    [SerializeField]
    private InputField regAccountInput;
    [SerializeField]
    private InputField regpwInput;
    [SerializeField]
    private InputField regpw1Input;
    #endregion
    [SerializeField]
    private Button loginBtn;
    [SerializeField]
    private Button regBtn;
    void Awake() {
        NetMessageUtil.I.Initial();
    }
    // Use this for initialization
    void Start () {
        loginBtn.onClick.AddListener(LoginOnClick);
        regBtn.onClick.AddListener(RegClick);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void LoginOnClick() {
        KCPSocket.I.ConnectServer();
        if (accountInput.text.Length == 0 || accountInput.text.Length > 6) {
            //WarrningManager.errors.Add(new WarrningModel("账号不合法"));
            Debug.Log("账号不合法");
            return;
        }
        if (passwordInput.text.Length == 0 || passwordInput.text.Length > 6) {
            Debug.Log("密码不合法");
            return;
        }
        //验证通过 申请登陆
        AccountInfoDTO dto = new AccountInfoDTO();
        dto.account = accountInput.text;
        dto.password = passwordInput.text;
        //this.WriteMessage(Protocol.TYPE_LOGIN, 0, LoginProtocol.LOGIN_CREQ, SerializeUtil.SerEncode(dto));
        List<AccountInfoDTO> accounts = new List<AccountInfoDTO>();
        for (int i = 0; i < 1; i++) {
            accounts.Add(dto);
        }
        for (int i = 0; i < 1; i++) {
            this.WriteMessage(0,SerializeUtil.SerEncode(accounts));
        }
    }

    public void RegClick() {
        if (regAccountInput.text.Length == 0 || regAccountInput.text.Length > 6) {
            Debug.Log("账号不合法");
            return;
        }
        if (regpwInput.text.Length == 0 || regpwInput.text.Length > 6) {
            Debug.Log("密码不合法");
            return;
        }
        if (!regpwInput.text.Equals(regpw1Input.text)) {
            Debug.Log("两次输入密码不一致");
            return;
        }
        //验证通过 申请注册 并关闭注册面板
        AccountInfoDTO dto = new AccountInfoDTO();
        dto.account = regAccountInput.text;
        dto.password = regpwInput.text;
        this.WriteMessage(0, SerializeUtil.SerEncode(dto));
    }
    /// <summary>游戏退出时候关闭socket </summary>
    private void OnApplicationQuit() {
        this.Close();
    }
}

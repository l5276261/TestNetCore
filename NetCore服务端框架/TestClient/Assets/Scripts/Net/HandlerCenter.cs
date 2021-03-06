﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlerCenter : InstanceMono<HandlerCenter> {
    Dictionary<int, string> nameDic;
    Dictionary<int, IHandler> handlerDic;
    private GameObject CanvasGO;
    IHandler login;
    //IHandler user;
    //IHandler match;
    //IHandler select;
    //IHandler fight;
    // Use this for initialization
    void Awake() {
        nameDic = Methods.LoadProtocol("protocol");
        handlerDic = Methods.GetIHandlerDic(nameDic);
        Ex.Type = NetType.KCP;
    }
    void Start() {
        CanvasGO = GameObject.Find("Canvas");
        login = new LoginHandler();
        //user = GetComponent<UserHandler>();
        //match = GetComponent<MatchHandler>();
        //select = GetComponent<SelectHandler>();
        //fight = GetComponent<FightHandler>();
        //    InvokeRepeating("checkMessage", 1f / 60, 1f / 60);
    }

    void Update() {
        if (Ex.Type == NetType.KCP)
            KcpClient.I.Update();
        while(this.Messages().Count > 0) {
            MessageModel model = this.Messages()[0];
            StartCoroutine("MessageReceive", model);
            this.Messages().RemoveAt(0);
        }
    }

    void MessageReceive(MessageModel model) {
        handlerDic[model.ID].Clone().MessageReceive(model);
    }
}

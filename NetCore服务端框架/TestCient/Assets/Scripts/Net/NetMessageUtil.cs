using GameProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMessageUtil :InstanceMono<NetMessageUtil> {

    private GameObject CanvasGO;
    IHandler login;
    //IHandler user;
    //IHandler match;
    //IHandler select;
    //IHandler fight;
    // Use this for initialization
    void Awake() {
        Ex.IsUdp = false;
        this.Initial();
    }
    void Start() {
        CanvasGO = GameObject.Find("Canvas");
        login = CanvasGO.GetComponent<LoginHandler>();
        //user = GetComponent<UserHandler>();
        //match = GetComponent<MatchHandler>();
        //select = GetComponent<SelectHandler>();
        //fight = GetComponent<FightHandler>();
        //    InvokeRepeating("checkMessage", 1f / 60, 1f / 60);
    }

    void Update() {
        while(this.Messages().Count > 0) {
            SocketModel model = this.Messages()[0];
            StartCoroutine("MessageReceive", model);
            this.Messages().RemoveAt(0);
        }
    }

    void MessageReceive(SocketModel model) {
        switch (model.Type) {
            case Protocol.TYPE_LOGIN:
                login.MessageReceive(model);
                break;
            //case Protocol.TYPE_USER:
            //    user.MessageReceive(model);
            //    break;
            //case Protocol.TYPE_MATCH:
            //    match.MessageReceive(model);
            //    break;
            //case Protocol.TYPE_SELECT:
            //    select.MessageReceive(model);
            //    break;
            //case Protocol.TYPE_FIGHT:
            //    fight.MessageReceive(model);
            //    break;
        }
    }
}

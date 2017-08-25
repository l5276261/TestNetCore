using GameProtocol;
using NetFrame;
using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.Text;
using TestCore.LOLServer.Logic;
using TestCore.LOLServer.Logic.Login;

namespace TestCore.LOLServer
{

    public class HandlerCenter : AbsHandlerCenter {
        HandlerInterface login;

        public HandlerCenter() {
            login = new LoginHandler();
        }
        public override void ClientClose(UserToken token, string error) {
            Console.WriteLine("有客户端断开连接了");
        }

        public override void ClientConnect(UserToken token) {
            Console.WriteLine("有客户端连接了");
        }

        public override void MessageReceive(UserToken token, object message) {
            SocketModel model = message as SocketModel;
            switch (model.Type) {
                case Protocol.TYPE_LOGIN:
                    login.MessageReceive(token, model);
                    break;
                case Protocol.TYPE_USER:
                    //user.MessageReceive(token, model);
                    break;
                case Protocol.TYPE_MATCH:
                    //match.MessageReceive(token, model);
                    break;
                case Protocol.TYPE_SELECT:
                    //select.MessageReceive(token, model);
                    break;
                case Protocol.TYPE_FIGHT:
                    //fight.MessageReceive(token, model);
                    break;
                default:
                    //未知模块  可能是客户端作弊了 无视
                    break;
            }
        }
    }
}

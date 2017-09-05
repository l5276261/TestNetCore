using Microsoft.Extensions.Configuration;
using NetFrame;
using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.Text;
using TestCore.Server.Logic;
using TestCore.Server.Logic.Login;
using TestCore.Tool;

namespace TestCore.Server
{

    public class HandlerCenter : AbsHandlerCenter {
        IConfigurationRoot appConfig;
        IConfigurationRoot protocolConfig;
        Dictionary<int, string> handlerNameDic;
        Dictionary<int, IHandler> handlerDic;
        IHandler login;

        public HandlerCenter() {
            login = new LoginHandler();
            appConfig = Methods.GetConfig("appsettings.json");
            protocolConfig = Methods.GetConfig("protocol.json");
            handlerNameDic = Methods.ConfigToDic(protocolConfig);
            handlerDic = Methods.GetIHandlerDic(appConfig["protocolPath"], handlerNameDic);
        }
        public override void ClientClose(UserToken token, string error) {
            Console.WriteLine("有客户端断开连接了");
        }

        public override void ClientConnect(UserToken token) {
            Console.WriteLine("有客户端连接了");
        }

        public override void MessageReceive(UserToken token, object message) {
            MessageModel model = message as MessageModel;
            handlerDic[model.ID].Clone().MessageReceive(token, message as MessageModel);
        }
    }
}

using Microsoft.Extensions.Configuration;
using NetFrame;
using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace HandlerModule
{
    public class HandlerCenter : AbsHandlerCenter {
        IConfigurationRoot appConfig;
        IConfigurationRoot protocolConfig;
        Dictionary<int, string> handlerNameDic;
        Dictionary<int, IHandler> handlerDic;

        public HandlerCenter() {
            appConfig = HandlerMethods.GetConfig("appsettings.json");
            protocolConfig = HandlerMethods.GetConfig("protocol.json");
            handlerNameDic = HandlerMethods.ConfigToDic(protocolConfig);
            handlerDic = HandlerMethods.GetIHandlerDic(appConfig["protocolPath"], handlerNameDic);
        }
        public override void ClientClose(UserToken token, string error) {
            Console.WriteLine("有客户端断开连接了");
        }

        public override void ClientConnect(UserToken token) {
            Console.WriteLine("有客户端连接了");
        }

        public override void MessageReceive(UserToken token, object message) {
            //TODO只解析业务ID，然后分发到对应的业务对象，再由其解析成具体的数据并执行业务。
            MessageModel model = message as MessageModel;
            handlerDic[model.ID].Clone().MessageReceive(token, message as MessageModel);
        }

        public override void UDPClientClose(SocketAsyncEventArgs e, string error) {
            Console.WriteLine("有客户端断开连接了");
        }
    }
}

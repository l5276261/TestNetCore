using System;
using System.Collections.Generic;
using System.Text;
using NetFrame;
using NetFrame.auto;
using TestCore.Server.Biz;
using TestCore.Server.dto;
using HandlerModule;

namespace TestCore.Server.Logic.Login
{
    public class LoginHandler : AbsOnceHandler, IHandler {
        string str;
        IAccountBiz accountBiz = BizFactory.accountBiz;
        public void ClientClose(UserToken token, string error) {

        }

        public void MessageReceive(UserToken token, MessageModel message) {
            List<AccountInfoDTO> accounts = SerializeUtil.DesDecode<List<AccountInfoDTO>>(message.Message as byte[]);
            string str = "";
            for (int i = 0; i < accounts.Count; i++) {
                str += " " + accounts[i].account;
            }
            Console.WriteLine(str);
            //Console.WriteLine("信息输出完成");
            write(token, 0, SerializeUtil.SerEncode(accounts));
            //message.Message = SerializeUtil.DesDecode<AccountInfoDTO>(message.Message as byte[]);
            //switch (message.Command) {
            //    case LoginProtocol.LOGIN_CREQ:
            //        login(token, message.GetMessage<AccountInfoDTO>());
            //        break;
            //    case LoginProtocol.REG_CREQ:
            //        reg(token, message.GetMessage<AccountInfoDTO>());
            //        break;
            //}
        }

        public void Set(object value) {
            str = (string)value;
            Console.WriteLine("得到消息 ");
        }

        public void Do() {
            Console.WriteLine("得到的消息是 " + str);
        }

        public IHandler Clone() {
            return new LoginHandler();
        }
    }
}

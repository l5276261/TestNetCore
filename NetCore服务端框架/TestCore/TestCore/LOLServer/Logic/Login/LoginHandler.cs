using System;
using System.Collections.Generic;
using System.Text;
using NetFrame;
using NetFrame.auto;
using GameProtocol;
using GameProtocol.dto;
using TestCore.LOLServer.Biz;
using TestCore.LOLServer.Tool;

namespace TestCore.LOLServer.Logic.Login
{
    public class LoginHandler : AbsOnceHandler, HandlerInterface {
        IAccountBiz accountBiz = BizFactory.accountBiz;
        public void ClientClose(UserToken token, string error) {
            ExecutorPool.Instance.execute(
         delegate () {
             accountBiz.close(token);
         }
         );
        }

        public void MessageReceive(UserToken token, SocketModel message) {
            message.Message = SerializeUtil.DesDecode<AccountInfoDTO>(message.Message as byte[]);
            switch (message.Command) {
                case LoginProtocol.LOGIN_CREQ:
                    login(token, message.GetMessage<AccountInfoDTO>());
                    break;
                case LoginProtocol.REG_CREQ:
                    reg(token, message.GetMessage<AccountInfoDTO>());
                    break;
            }
        }
        public void login(UserToken token, AccountInfoDTO value) {
            ExecutorPool.Instance.execute(
                delegate () {
                    int result = accountBiz.login(token, value.account, value.password);
                    write(token, LoginProtocol.LOGIN_SRES, SerializeUtil.SerEncode(result));
                }
                );
        }
        public void reg(UserToken token, AccountInfoDTO value) {
            ExecutorPool.Instance.execute(
                    delegate () {
                        int result = accountBiz.create(token, value.account, value.password);
                        write(token, LoginProtocol.REG_SRES, SerializeUtil.SerEncode(result));
                    }
                    );
        }

        public override byte GetType() {
            return Protocol.TYPE_LOGIN;
        }
    }
}

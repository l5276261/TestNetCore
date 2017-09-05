using NetFrame;
using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCore.Server.Logic
{
    public interface IHandler {
        void ClientClose(UserToken token, string error);

        //   void ClientConnect(UserToken token);

        void MessageReceive(UserToken token, MessageModel message);
        void Set(object value);
        void Do();
        IHandler Clone();
    }
}

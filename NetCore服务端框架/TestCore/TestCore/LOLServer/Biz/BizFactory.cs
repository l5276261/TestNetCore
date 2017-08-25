using System;
using System.Collections.Generic;
using System.Text;
using TestCore.LOLServer.Biz.Impl;

namespace TestCore.LOLServer.Biz
{
    public class BizFactory {
        public readonly static IAccountBiz accountBiz;
        public readonly static IUserBiz userBiz;
        static BizFactory() {
            accountBiz = new AccountBiz();
            userBiz = new UserBiz();
        }
    }
}

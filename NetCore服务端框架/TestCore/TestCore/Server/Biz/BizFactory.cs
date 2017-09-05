using System;
using System.Collections.Generic;
using System.Text;
using TestCore.Server.Biz.Impl;

namespace TestCore.Server.Biz
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

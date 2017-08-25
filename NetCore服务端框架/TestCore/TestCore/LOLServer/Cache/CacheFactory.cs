using System;
using System.Collections.Generic;
using System.Text;
using TestCore.LOLServer.Cache.Impl;

namespace TestCore.LOLServer.Cache
{
    public class CacheFactory {
        public readonly static IAccountCache accountCache;

        public readonly static IUserCache userCache;
        static CacheFactory() {
            accountCache = new AccountCache();
            userCache = new UserCache();
        }
    }
}

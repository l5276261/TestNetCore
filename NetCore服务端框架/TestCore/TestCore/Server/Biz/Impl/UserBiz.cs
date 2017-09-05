using System;
using System.Collections.Generic;
using System.Text;
using NetFrame;
using TestCore.Server.Dao.Model;
using TestCore.Server.Cache;

namespace TestCore.Server.Biz.Impl
{
    public class UserBiz : IUserBiz {
        IAccountBiz accBiz = BizFactory.accountBiz;
        IUserCache userCache = CacheFactory.userCache;
        public bool Create(NetFrame.UserToken token, string name) {
            //帐号是否登陆 获取帐号ID
            int accountId = accBiz.get(token);
            if (accountId == -1) return false;
            //判断当前帐号是否已经拥有角色
            if (userCache.hasByAccountId(accountId)) return false;
            userCache.create(token, name, accountId);
            return true;
        }

        public USER getByAccount(NetFrame.UserToken token) {
            //帐号是否登陆 获取帐号ID
            int accountId = accBiz.get(token);
            if (accountId == -1) return null;
            return userCache.getByAccountId(accountId);
        }

        public USER get(int id) {
            return userCache.get(id);
        }

        public USER online(UserToken token) {
            int accountId = accBiz.get(token);
            if (accountId == -1) return null;
            USER user = userCache.getByAccountId(accountId);
            if (userCache.isOnline(user.id)) return null;
            userCache.online(token, user.id);
            return user;
        }

        public void offline(UserToken token) {
            userCache.offline(token);
        }

        public UserToken getToken(int id) {
            return userCache.getToken(id);
        }


        public USER get(UserToken token) {
            return userCache.get(token);
        }
    }
}

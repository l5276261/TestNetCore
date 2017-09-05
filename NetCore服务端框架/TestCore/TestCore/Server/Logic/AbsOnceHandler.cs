using NetFrame;
using NetFrame.auto;
using System;
using System.Collections.Generic;
using System.Text;
using TestCore.Server.Biz;
using TestCore.Server.Model;

namespace TestCore.Server.Logic
{
    public class AbsOnceHandler {
        public IUserBiz userBiz = BizFactory.userBiz;
        /// <summary>
        /// 通过连接对象获取用户
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public USER getUser(UserToken token) {
            return userBiz.get(token);
        }

        /// <summary>
        /// 通过ID获取用户
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public USER getUser(int id) {
            return userBiz.get(id);
        }

        /// <summary>
        /// 通过连接对象 获取用户ID
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public int getUserId(UserToken token) {
            USER user = getUser(token);
            if (user == null) return -1;
            return user.id;
        }
        /// <summary>
        /// 通过用户ID获取连接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UserToken getToken(int id) {
            return userBiz.getToken(id);
        }


        #region 通过连接对象发送
        public void write(UserToken token, int id) {
            write(token, id, null);
        }
        public void write(UserToken token,int id, object message) {
            byte[] value = MessageEncoding.Encode(CreateSocketModel(id, message));
            value = LengthEncoding.Encode(value);
            token.Write(value);
        }
        #endregion

        #region 通过ID发送
        public void write(int id) {
            write(id, null);
        }
        public void write(int id, object message) {
            UserToken token = getToken(id);
            if (token == null) return;
            write(token, id, message);
        }

        public void writeToUsers(int[] users,int id, object message) {
            byte[] value = MessageEncoding.Encode(CreateSocketModel(id, message));
            value = LengthEncoding.Encode(value);
            foreach (int item in users) {
                UserToken token = userBiz.getToken(item);
                if (token == null) continue;
                byte[] bs = new byte[value.Length];
                Array.Copy(value, 0, bs, 0, value.Length);
                token.Write(bs);
            }
        }


        #endregion





        public MessageModel CreateSocketModel(int id, object message) {
            return new MessageModel(id, message);
        }
    }
}

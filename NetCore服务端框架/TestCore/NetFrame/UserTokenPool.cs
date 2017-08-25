using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame
{
    public class UserTokenPool
    {
        private Stack<UserToken> pool;
        public UserTokenPool(int max) {
            pool = new Stack<UserToken>(max);
        }
        /// <summary>取出一个连接对象--创建连接 </summary>
        public UserToken Pop() {
            return pool.Pop();
        }
        /// <summary>插入一个连接对象--释放连接 </summary>
        public void Push(UserToken token) {
            if (token != null) pool.Push(token);
        }
        public int Size {
            get { return pool.Count; }
        }
    }
}

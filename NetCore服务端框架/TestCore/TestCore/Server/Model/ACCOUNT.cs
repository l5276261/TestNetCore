using System;
using System.Collections.Generic;
using System.Text;

namespace TestCore.Server.Model
{
    /// <summary>
    /// 类ACCOUNT。
    /// </summary>
    [Serializable]
    public partial class ACCOUNT {
        public ACCOUNT() { }
        #region Model
        private int _id = -1;
        private string _account;
        private string _password;
        /// <summary>
        /// 
        /// </summary>
        public int id {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string account {
            set { _account = value; }
            get { return _account; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string password {
            set { _password = value; }
            get { return _password; }
        }
        #endregion Model
        #region Method
        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public ACCOUNT(string account) {

        }
        /// <summary>
		/// 增加一条数据
		/// </summary>
        public void Add() {

        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TestCore.Server.Model
{
    /// <summary>
    /// 类USER。
    /// </summary>
    [Serializable]
    public partial class USER {
        public USER() { }
        #region Model
        private int _id = -1;
        private string _name;
        private int _level = 1;
        private int _exp = 0;
        private int _win = 0;
        private int _lose = 0;
        private int _ran = 0;
        private int _accountid;
        private int[] _herolist;
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
        public string name {
            set { _name = value; }
            get { return _name; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int level {
            set { _level = value; }
            get { return _level; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int exp {
            set { _exp = value; }
            get { return _exp; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int win {
            set { _win = value; }
            get { return _win; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int lose {
            set { _lose = value; }
            get { return _lose; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int ran {
            set { _ran = value; }
            get { return _ran; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int accountId {
            set { _accountid = value; }
            get { return _accountid; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int[] heroList {
            set { _herolist = value; }
            get { return _herolist; }
        }

        #endregion Model
        #region Method
        /// <summary>
		/// 得到一个对象实体
		/// </summary>
		public USER(int accountId) {

        }
        /// <summary>
		/// 增加一条数据
		/// </summary>
		public void Add() {

        }
        #endregion
    }
}

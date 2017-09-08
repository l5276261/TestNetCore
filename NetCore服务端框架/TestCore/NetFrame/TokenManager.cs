using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NetFrame
{
    public static class TokenManager
    {
        /// <summary>UDP管理连接对象token的ID和连接对象Token的映射关系字典 </summary>
        public static ConcurrentDictionary<int, UserToken> Tcp_TokenDic = new ConcurrentDictionary<int, UserToken>();
        /// <summary>UDP管理连接对象token的ID和连接对象Token的映射关系字典 </summary>
        public static ConcurrentDictionary<int, UserToken> Udp_TokenDic = new ConcurrentDictionary<int, UserToken>();
        /// <summary>Kcp对象ID和连接对象Token的映射关系字典，为查询使用 </summary>
        public static ConcurrentDictionary<UInt32, UserToken> Kcp_TokenDic = new ConcurrentDictionary<UInt32, UserToken>();
        // <summary>Kcp对象list，为定时遍历刷新使用，由于多线程问题，需要设计如何跟KCP融合，尽量减少锁的使用 </summary>
        //public static List<UserToken> Kcp_TokenList = new List<UserToken>();
    }
}

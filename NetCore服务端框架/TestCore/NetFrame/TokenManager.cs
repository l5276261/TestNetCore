using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NetFrame
{
    public static class TokenManager
    {
        /// <summary>UDP管理连接对象token的ID和连接对象Token的映射关系字典 </summary>
        public static ConcurrentDictionary<string, UserToken> Tcp_TokenDic = new ConcurrentDictionary<string, UserToken>();
        /// <summary>UDP管理连接对象token的ID和连接对象Token的映射关系字典 </summary>
        public static ConcurrentDictionary<string, UserToken> Udp_TokenDic = new ConcurrentDictionary<string, UserToken>();
        /// <summary>Kcp对象ID和连接对象Token的映射关系字典 </summary>
        public static ConcurrentDictionary<UInt32, UserToken> Kcp_TokenDic = new ConcurrentDictionary<UInt32, UserToken>();
    }
}

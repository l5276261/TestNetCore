using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameProtocol.dto
{
    [Serializable, ProtoContract]//前者是BinaryFormatter对象序列化需要的，不过已经决定放弃使用他序列化对象了。后者是PB序列化的标签
    public class AccountInfoDTO {
        [ProtoMember(1)]
        public string account;
        [ProtoMember(2)]
        public string password;
    }
}

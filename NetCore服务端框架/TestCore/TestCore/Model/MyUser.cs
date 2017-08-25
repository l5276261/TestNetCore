using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCore.Model {
    [ProtoContract, Serializable]
    public class MyUser  {
        [ProtoMember(1)]
        public int UserId { get; set; }
        [ProtoMember(2)]
        public string PassportID { get; set; }
        [ProtoMember(3)]
        public DateTime RegTime { get; set; }
        [ProtoMember(4)]
        public string PassportPwd { get; set; }
        [ProtoMember(5)]
        public string DeviceID { get; set; }
        [ProtoMember(9)]
        public int ServerID { get; set; }
        [ProtoMember(10)]
        public string Phone { get; set; }
    }
}

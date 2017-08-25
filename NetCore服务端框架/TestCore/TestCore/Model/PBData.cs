using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace TestCore.Model {
    [ProtoContract]
    public class PBData {
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public int Age;
        [ProtoMember(3)]
        public bool IsBoy;
        [ProtoMember(4)]
        public PBData Data;
    }
}

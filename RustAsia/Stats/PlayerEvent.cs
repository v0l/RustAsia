using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace RustAsia.Stats
{
    [ProtoContract]
    public class PlayerEvent : BaseStat
    {
        [ProtoMember(1)]
        public ulong SteamId { get; set; }

        [ProtoMember(2)]
        public string DisplayName { get; set; }
    }
}

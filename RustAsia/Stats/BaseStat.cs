using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace RustAsia.Stats
{
    [ProtoContract]
    [ProtoInclude(10, typeof(PlayerEvent))]
    public class BaseStat
    {
        [ProtoMember(1)]
        public DateTime LogTime { get; set; } = DateTime.Now;
    }
}

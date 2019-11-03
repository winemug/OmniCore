using SQLite;
using System;

namespace OmniCore.Repository.Entities
{
    public class AlertStateResponse : Entity
    {
        [Indexed]
        public long RequestId {get; set;}
        public uint AlertW278 { get; set; }
        public uint[] AlertStates { get; set; }
    }
}

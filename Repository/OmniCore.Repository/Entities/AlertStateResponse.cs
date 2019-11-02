using System;

namespace OmniCore.Repository.Entities
{
    public class AlertStateResponse : Entity
    {
        public long RequestId {get; set;}
        public uint AlertW278 { get; set; }
        public uint[] AlertStates { get; set; }
    }
}

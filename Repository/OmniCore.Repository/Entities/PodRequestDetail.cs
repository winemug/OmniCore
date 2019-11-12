using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class PodRequestDetail : Entity
    {
        public int PodRequestId { get; set; }
        public bool Successful { get; set; }
        public byte[] PacketSent { get; set; }
        public byte[] PacketReceived { get; set; }
        public int Retries { get; set; }
    }
}

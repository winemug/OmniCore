using System;
using OmniCore.Repository.Enums;

namespace OmniCore.Repository.Entities
{
    public class PodStatus : Entity
    {
        public long RequestId {get; set;}
        public bool Faulted { get; set; }
        public decimal NotDeliveredUnits { get; set; }
        public decimal DeliveredUnits { get; set; }
        public decimal ReservoirUnits { get; set; }
        public PodProgress Progress { get; set; }
        public BasalState BasalState { get; set; }
        public BolusState BolusState { get; set; }
        public uint ActiveMinutes { get; set; }
        public byte AlertMask { get; set; }
    }
}

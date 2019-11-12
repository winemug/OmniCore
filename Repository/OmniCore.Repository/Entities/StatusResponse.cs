using System;
using Newtonsoft.Json;
using OmniCore.Repository.Enums;
using SQLite;

namespace OmniCore.Repository.Entities
{
    public class StatusResponse : Entity
    {
        [Indexed]
        public long RequestId {get; set;}

        public bool Faulted { get; set; }
        public int MessageSequence { get; set; }
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

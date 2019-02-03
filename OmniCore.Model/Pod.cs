using Newtonsoft.Json;
using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    [Serializable]
    public class Pod
    {
        public uint? Lot { get; set; }
        public uint? Tid { get; set; }
        public uint? Address { get; set; }
        public DateTime? LastUpdated { get; set; }
        public PodProgress? Progress { get; set; }
        public BasalState? BasalDelivery { get; set; }
        public BolusState? BolusDelivery { get; set; }
        public Alarm? Alarms { get; set; }
        public int? DeliveredPulses { get; set; }
        public int? NotDeliveredPulses { get; set; }
        public int? MessageSequence { get; set; }
        public bool? Faulted { get; set; }
        public int? ActiveMinutes { get; set; }
        public int? Reservoir { get; set; }
    }
}

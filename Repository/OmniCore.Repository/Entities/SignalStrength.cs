using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class SignalStrength : Entity
    {
        public long RadioId { get; set; }
        public long? PodId { get; set; }
        public int? ClientRadioRssi { get; set; }
        public int? RadioPodSsi { get; set; }
        public int? PodRadioSsi { get; set; }
        public int? PodLowGain { get; set; }
    }
}

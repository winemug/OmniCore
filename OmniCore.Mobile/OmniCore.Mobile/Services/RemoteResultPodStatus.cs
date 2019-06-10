using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Services
{
    public class RemoteResultPodStatus
    {
        public string StatusText { get; set; }
        public string PodId { get; set; }
        public bool PodRunning { get; set; }
        public double ReservoirLevel { get; set; }
        public double InsulinCanceled { get; set; }

        public decimal[] BasalSchedule { get; set; }
        public int UtcOffset { get; set; }
        public long LastUpdated { get; set; }
        public long ResultId { get; set; }
    }
}

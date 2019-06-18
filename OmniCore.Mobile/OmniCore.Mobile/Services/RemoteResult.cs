using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Services
{
    public class RemoteResult
    {
        public bool Success { get; set; }
        public bool PodRunning { get; set; }
        public string PodId { get; set; }

        public long ResultDate { get; set; }

        public decimal[] BasalSchedule { get; set; }
        public int UtcOffset { get; set; }

        public decimal InsulinCanceled { get; set; }
        public decimal ReservoirLevel { get; set; }
        public int BatteryLevel { get; set; }

        public long LastResultDateTime { get; set; }
        public HistoricalResult[] ResultsToDate { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public RemoteResult WithSuccess()
        {
            Success = true;
            return this;
        }
    }
}

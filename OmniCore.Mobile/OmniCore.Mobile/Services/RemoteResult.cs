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
        public long? ResultId { get; set; }

        public RemoteResultPodStatus Status { get; set; }
        public HistoricalResult[] ResultsToDate { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

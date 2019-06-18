using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Services
{
    public class RemoteRequest
    {
        public RemoteRequestType Type { get; set; }

        public int UtcOffsetMinutes { get; set; }
        public decimal[] BasalSchedule { get; set; }

        public decimal ImmediateUnits { get; set; }

        public decimal TemporaryRate { get; set; }
        public decimal DurationHours { get; set; }

        public long LastResultDateTime { get; set; }

        public static RemoteRequest FromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<RemoteRequest>(jsonString);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

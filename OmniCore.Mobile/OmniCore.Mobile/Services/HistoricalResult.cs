using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Services
{
    public class HistoricalResult
    {
        public long ResultId { get; set; }
        public long ResultDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public HistoricalResultType Type { get; set; }

        public bool PodRunning { get; set; }
        public string Parameters { get; set; }
    }
}

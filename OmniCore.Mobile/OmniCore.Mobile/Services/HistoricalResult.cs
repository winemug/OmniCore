using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Services
{
    public class HistoricalResult
    {
        public long ResultId { get; set; }
        public long ResultDate { get; set; }
        public HistoricalResultType Type { get; set; }

        public bool PodRunning { get; set; }
        public string Parameters { get; set; }
    }
}

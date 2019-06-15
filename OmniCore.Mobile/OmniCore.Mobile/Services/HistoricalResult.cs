using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Services
{
    public class HistoricalResult
    {
        public long ResultId { get; set; }
        public HistoricalResultType Type { get; set; }
        public RemoteResultPodStatus Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Repository.Enums;

namespace OmniCore.Repository.Entities
{
    public class PodRequest : UpdateableEntity
    {
        public long PodId { get; set; }
        public RequestState RequestStatus { get; set; }
        public RequestType RequestType { get; set; }
        public string Parameters { get; set; }
        public DateTimeOffset? StartEarliest { get; set; }
        public DateTimeOffset? StartLatest { get; set; }

        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? ResultReceived { get; set; }

        public FailureType? FailureType { get; }
        public string ExceptionText { get; }

        public int QueueWaitDuration { get; set; }
        public int ExchangeDuration { get; set; }
        public int TotalRadioOverhead { get; set; }
        public int PacketExchangeCount { get; set; }
        public int PacketExchangeDurationAverage { get; set; }
        public int? RadioRssiAverage { get; set; }
        public int? MobileDeviceRssiAverage { get; set; }
        public int? PodRssi { get; set; }
        public int? PodLowGain { get; set; }
        public int PowerAdjustmentCount { get; set; }
    }
}

using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model
{
    public class MessageExchangeStatistics : IMessageExchangeStatistics
    {
        [PrimaryKey]
        public long? Id { get; set; }
        public long ResultId { get; set; }

        public int QueueWaitDuration { get; set; }

        public int ExchangeDuration { get; set; }

        public int TotalRadioOverhead { get; set; }

        public int PacketExchangeCount { get; set; }

        public int PacketExchangeDurationAverage { get; set; }

        public int PodRssiAverage { get; set; }

        public int RadioRssiAverage { get; set; }
    }
}

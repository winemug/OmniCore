using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeStatistics
    {
        [PrimaryKey]
        long? Id { get; set; }
        long ResultId { get; set; }

        int QueueWaitDuration { get; set; }
        int ExchangeDuration { get; set; }

        int TotalRadioOverhead { get; set; }

        int PacketExchangeCount { get; set; }
        int PacketExchangeDurationAverage { get; set; }

        int PodRssiAverage { get; set; }
        int RadioRssiAverage { get; set; }
    }
}

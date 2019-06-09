using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeStatistics
    {
        int QueueWaitDuration { get; }
        int ExchangeDuration { get; }

        int TotalRadioOverhead { get; }

        int PacketExchangeCount { get; }
        int PacketExchangeDurationAverage { get; }

        int PodRssiAverage { get; }
        int RadioRssiAverage { get; }
    }
}

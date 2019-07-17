using System;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeStatistics
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTimeOffset Created { get; set; }
        int QueueWaitDuration { get; set; }
        int ExchangeDuration { get; set; }
        int TotalRadioOverhead { get; set; }
        int PacketExchangeCount { get; set; }
        int PacketExchangeDurationAverage { get; set; }
        int? RadioRssiAverage { get; set; }
        int? MobileDeviceRssiAverage { get; set; }
        int? PodRssi { get; set; }
        int? PodLowGain { get; set; }
        int PowerAdjustmentCount { get; set; }

        void BeforeSave();
    }
}

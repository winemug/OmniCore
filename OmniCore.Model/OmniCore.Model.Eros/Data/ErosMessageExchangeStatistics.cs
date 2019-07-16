using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros.Data
{
    public class ErosMessageExchangeStatistics : IMessageExchangeStatistics
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTimeOffset Created { get; set; }

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

        public int? PacketErrors { get; set; }

        public virtual void BeforeSave()
        {
        }
    }
}

using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros.Data
{
    public class ErosMessageExchangeStatistics : PropertyChangedImpl, IMessageExchangeStatistics
    {
        private DateTimeOffset created;
        private int queueWaitDuration;
        private int exchangeDuration;
        private int totalRadioOverhead;
        private int packetExchangeCount;
        private int packetExchangeDurationAverage;
        private int? radioRssiAverage;
        private int? mobileDeviceRssiAverage;
        private int? podRssi;
        private int? podLowGain;

        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTimeOffset Created { get => created; set => SetProperty(ref created, value); }

        public int QueueWaitDuration { get => queueWaitDuration; set => SetProperty(ref queueWaitDuration, value); }

        public int ExchangeDuration { get => exchangeDuration; set => SetProperty(ref exchangeDuration, value); }

        public int TotalRadioOverhead { get => totalRadioOverhead; set => SetProperty(ref totalRadioOverhead, value); }

        public int PacketExchangeCount { get => packetExchangeCount; set => SetProperty(ref packetExchangeCount, value); }

        public int PacketExchangeDurationAverage { get => packetExchangeDurationAverage; set => SetProperty(ref packetExchangeDurationAverage, value); }

        public int? RadioRssiAverage { get => radioRssiAverage; set => SetProperty(ref radioRssiAverage, value); }
        public int? MobileDeviceRssiAverage { get => mobileDeviceRssiAverage; set => SetProperty(ref mobileDeviceRssiAverage, value); }

        public int? PodRssi { get => podRssi; set => SetProperty(ref podRssi, value); }
        public int? PodLowGain { get => podLowGain; set => SetProperty(ref podLowGain, value); }
        public int PowerAdjustmentCount { get; set; }

        public virtual void BeforeSave()
        {
        }
    }
}

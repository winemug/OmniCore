using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodStatus : IPodStatus
    {
        [PrimaryKey, AutoIncrement]
        public long? Id  { get; set; }
        public long ResultId { get; set; }

        public bool Faulted  { get; set; }
        public decimal NotDeliveredInsulin  { get; set; }
        public decimal DeliveredInsulin  { get; set; }
        public decimal Reservoir  { get; set; }
        public PodProgress Progress  { get; set; }
        public BasalState BasalState  { get; set; }
        public BolusState BolusState  { get; set; }
        public uint ActiveMinutes  { get; set; }
        public byte AlertMask  { get; set; }

        private int _message_seq = 0;
        public int MessageSequence
        {
            get => _message_seq;
            set
            {
                _message_seq = value % 16;
            }
        }

        public ErosPodStatus()
        {
            Created = DateTime.UtcNow;
        }
    }
}

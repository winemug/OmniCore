using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public abstract partial class Pod : IPodVariables
    {
        public uint? Lot { get; set; }
        public uint? Serial { get; set; }
        public string VersionPm { get; set; }
        public string VersionPi { get; set; }
        public byte? VersionByte { get; set; }
        public byte[] Version7Bytes { get; set; }

        public uint RadioAddress { get; set; }

        private int _packet_seq = 0;
        private int _message_seq = 0;

        public int PacketSequence
        {
            get => _packet_seq;
            set
            {
                _packet_seq = value % 32;
            }
        }

        public int MessageSequence
        {
            get => _message_seq;
            set
            {
                _message_seq = value % 16;
            }
        }

        public int? RadioLowGain { get; set; }
        public int? RadioRssi { get; set; }

        public uint? LastNonce { get; set; }
        public uint NonceSeed { get; set; }
        public uint? NonceSync { get; set; }
        public int NoncePtr { get; set; }
        public int NonceRuns { get; set; }

        public DateTime? LastUpdated { get; set; }
        public PodProgress Progress { get; set; }
        public BasalState BasalState { get; set; }
        public BolusState BolusState { get; set; }
        public byte AlertMask { get; set; }
        public ushort? AlertW278 { get; set; }
        public ushort[] AlertStates { get; set; }
        public uint ActiveMinutes { get; set; }
        public bool Faulted { get; set; }

        public decimal? ReservoirAlertLevel { get; set; }
        public int? ExpiryAlertMinutes { get; set; }

        public decimal[] BasalSchedule { get; set; }
        public int? FaultCode { get; set; }
        public int? FaultRelativeTime { get; set; }
        public int? TableAccessFault { get; set; }
        public int? InsulinStateTableCorruption { get; set; }
        public int? InternalFaultVariables { get; set; }
        public bool? FaultedWhileImmediateBolus { get; set; }
        public PodProgress? ProgressBeforeFault { get; set; }
        public PodProgress? ProgressBeforeFault2 { get; set; }
        public uint? FaultInformation2LastWord { get; set; }

        public decimal Reservoir { get; set; }
        public decimal DeliveredInsulin { get; set; }
        public decimal NotDeliveredInsulin { get; set; }

        public int? UtcOffset { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? InsertionDate { get; set; }
    }
}
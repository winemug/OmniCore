using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodVariables
    {
        int? FaultCode { get; set; }
        int? FaultRelativeTime { get; set; }
        bool? FaultedWhileImmediateBolus { get; set; }
        uint? FaultInformation2LastWord { get; set; }
        int? InsulinStateTableCorruption { get; set; }
        int? InternalFaultVariables { get; set; }
        PodProgress? ProgressBeforeFault { get; set; }
        PodProgress? ProgressBeforeFault2 { get; set; }
        int? TableAccessFault { get; set; }
        uint? Lot { get; set; }
        uint? Serial { get; set; }
        string VersionPi { get; set; }
        string VersionPm { get; set; }
        byte[] Version7Bytes { get; set; }
        byte? VersionByte { get; set; }
        decimal NotDeliveredInsulin { get; set; }
        decimal DeliveredInsulin { get; set; }
        decimal Reservoir { get; set; }
        uint? LastNonce { get; set; }
        int NoncePtr { get; set; }
        int NonceRuns { get; set; }
        uint NonceSeed { get; set; }
        uint? NonceSync { get; set; }
        uint RadioAddress { get; set; }
        int? RadioLowGain { get; set; }
        int MessageSequence { get; set; }
        int PacketSequence { get; set; }
        int? RadioRssi { get; set; }
        uint ActiveMinutes { get; set; }
        byte AlertMask { get; set; }
        ushort? AlertW278 { get; set; }
        ushort[] AlertStates { get; set; }
        BasalState BasalState { get; set; }
        BolusState BolusState { get; set; }
        bool Faulted { get; set; }
        DateTime? LastUpdated { get; set; }
        PodProgress Progress { get; set; }
        DateTime? ActivationDate { get; set; }
        decimal? ReservoirAlertLevel { get; set; }
        int? ExpiryAlertMinutes { get; set; }
        decimal[] BasalSchedule { get; set; }
        DateTime? InsertionDate { get; set; }
        int? UtcOffset { get; set; }
    }
}

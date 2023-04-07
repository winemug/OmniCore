using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Common.Pod;

public class PodStatusModel
{
    bool Faulted { get; set; }
    public bool ExtendedBolusActive { get; init; }
    bool ImmediateBolusActive { get; init; }
    bool TempBasalActive { get; init; }
    bool BasalActive { get; init; }
    public int PulsesDelivered { get; init; }
    public int PulsesPending { get; init; }
    public int PulsesRemaining { get; init; }
    public int ActiveMinutes { get; init; }
    public int UnackedAlertsMask { get; set; }
}

public class PodFaultInfoModel
{
    public int FaultEventCode { get; }
    public int FaultEventMinute { get; }
    bool FaultPulseInformationInvalid { get; }
    bool FaultOcclusion { get; }
    bool FaultInsulinStateTable { get; }
    public int FaultOcclusionType { get; }
    bool FaultDuringImmediateBolus { get; }
    PodProgress ProgressBeforeFault { get; }
    public int LastProgrammingCommandSequence { get; }
}

public class PodVersionModel
{
    public int HardwareVersionMajor { get; init; }
    public int HardwareVersionMinor { get; init; }
    public int HardwareVersionRevision { get; init; }
    public int FirmwareVersionMajor { get; init; }
    public int FirmwareVersionMinor { get; init; }
    public int FirmwareVersionRevision { get; init; }
    public int ProductId { get; init; }
    public uint Lot { get; init; }
    public uint Serial { get; init; }
    public uint AssignedAddress { get; init; }
}

public class PodRadioMeasurementsModel
{
    public int RadioLowGain { get; init; }
    public int Rssi { get; init; }
    DateTimeOffset Received { get; }
}

public class PodActivationParametersModel
{
    public int PulseVolumeMicroUnits { get; init; }
    public int PulseRatePer125ms { get; init; }
    public int PrimingPulseRatePer125ms { get; init; }
    public int PrimingPulseCount { get; init; }
    public int CannulaInsertPulseCount { get; init; }
    public int MaximumLifeTimeHours { get; init; }
}

public class PodTimeModel
{
    public DateTimeOffset ValueWhen { get; set; }
    public TimeOnly Value { get; set; }
    public TimeOnly Then(DateTimeOffset when)
    {
        var timeDifference = DateTimeOffset.UtcNow - when;
        return Value.Add(timeDifference);
    }

    public TimeOnly Now
    {
        get
        {
            var timeDifference = DateTimeOffset.UtcNow - ValueWhen;
            return Value.Add(timeDifference);
        }
    }
}
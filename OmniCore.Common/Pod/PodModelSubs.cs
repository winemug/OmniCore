using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Common.Pod;

public class PodProgressModel
{
    public PodProgress Progress { get; set; }
    public bool Faulted { get; set; }
}

public class PodStatusModel
{
    public bool ExtendedBolusActive { get; init; }
    public bool ImmediateBolusActive { get; init; }
    public bool TempBasalActive { get; init; }
    public bool BasalActive { get; init; }
    public int PulsesDelivered { get; init; }
    public int PulsesPending { get; init; }
    public int PulsesRemaining { get; init; }
    public int ActiveMinutes { get; init; }
    public int UnackedAlertsMask { get; set; }
    public int LastProgrammingCommandSequence { get; set; }
}

public class PodFaultInfoModel
{
    public int FaultEventCode { get; init; }
    public int FaultEventMinute { get; init; }
    public bool FaultPulseInformationInvalid { get; init; }
    public bool FaultOcclusion { get; init; }
    public bool FaultInsulinStateTable { get; init; }
    public int FaultOcclusionType { get; init; }
    public bool FaultDuringImmediateBolus { get; init; }
    public PodProgress ProgressBeforeFault { get; init; }
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
    private DateTimeOffset Received { get; set; }
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

public class PodBasalModel
{
    public DateTimeOffset PodTimeReference { get; set; }
    public TimeOnly PodTimeReferenceValue { get; set; }
    public TimeOnly PodTimeThen(DateTimeOffset when)
    {
        var timeDifference = DateTimeOffset.UtcNow - when;
        return PodTimeReferenceValue.Add(timeDifference);
    }
    public TimeOnly PodTimeNow
    {
        get
        {
            var timeDifference = DateTimeOffset.UtcNow - PodTimeReference;
            return PodTimeReferenceValue.Add(timeDifference);
        }
    }
    
    public int[] BasalSchedule { get; set; }
}
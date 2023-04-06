using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Common.Pod;

public interface IPodStatusModel
{
    PodProgress Progress { get; }
    bool Faulted { get; set; }
    bool ExtendedBolusActive { get; }
    bool ImmediateBolusActive { get; }
    bool TempBasalActive { get; }
    bool BasalActive { get; }
    int PulsesDelivered { get; }
    int PulsesPending { get; }
    int PulsesRemaining { get; }
    int ActiveMinutes { get; }
    int UnackedAlertsMask { get; set; }
    DateTimeOffset Updated { get; }
}

public interface IPodFaultInfoModel
{
    int FaultEventCode { get; }
    int FaultEventMinute { get; }
    bool FaultPulseInformationInvalid { get; }
    bool FaultOcclusion { get; }
    bool FaultInsulinStateTable { get; }
    int FaultOcclusionType { get; }
    bool FaultDuringImmediateBolus { get; }
    PodProgress ProgressBeforeFault { get; }
    int LastProgrammingCommandSequence { get; }
}

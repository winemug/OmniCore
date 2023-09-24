namespace OmniCore.Common.Pod;

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
namespace OmniCore.Common.Pod;

public class PodActivationParametersModel
{
    public int PulseVolumeMicroUnits { get; init; }
    public int PulseRatePer125ms { get; init; }
    public int PrimingPulseRatePer125ms { get; init; }
    public int PrimingPulseCount { get; init; }
    public int CannulaInsertPulseCount { get; init; }
    public int MaximumLifeTimeHours { get; init; }
}
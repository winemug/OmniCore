namespace OmniCore.Shared.Entities;

public class PodActivationParametersModel
{
    public int PulseVolumeMicroUnits { get; init; }
    public int PulseRatePer125Ms { get; init; }
    public int PrimingPulseRatePer125Ms { get; init; }
    public int PrimingPulseCount { get; init; }
    public int CannulaInsertPulseCount { get; init; }
    public int MaximumLifeTimeHours { get; init; }
}
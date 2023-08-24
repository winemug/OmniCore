namespace OmniCore.Common.Pod;

public struct BolusEntry
{
    public int ImmediatePulseCount { get; set; }
    public int ImmediatePulseInterval125Ms { get; set; }
    public int ExtendedHalfHourCount { get; set; }
    public int ExtendedTotalPulseCount { get; set; }
}
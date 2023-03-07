namespace OmniCore.Services.Interfaces.Entities;

public struct BolusEntry
{
    public int ImmediatePulseCount { get; set; }
    public int ImmediatePulseInterval125ms { get; set; }
    public int ExtendedHalfHourCount { get; set; }
    public int ExtendedTotalPulseCount { get; set; }
}
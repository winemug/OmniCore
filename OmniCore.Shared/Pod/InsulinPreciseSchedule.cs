namespace OmniCore.Common.Pod;

public struct InsulinPreciseSchedule
{
    public decimal Units { get; set; }
    public TimeSpan Duration { get; set; }
}
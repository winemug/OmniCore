namespace OmniCore.Shared.Entities;

public struct InsulinPreciseSchedule
{
    public decimal Units { get; set; }
    public TimeSpan Duration { get; set; }
}
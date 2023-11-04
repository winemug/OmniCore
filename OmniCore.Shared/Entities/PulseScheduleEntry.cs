namespace OmniCore.Shared.Entities;

public struct PulseScheduleEntry
{
    public ushort CountDecipulses { get; set; }
    public uint IntervalMicroseconds { get; set; }
}
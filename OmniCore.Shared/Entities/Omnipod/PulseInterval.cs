namespace OmniCore.Shared.Entities.Omnipod;
public record PulseInterval
{
    public ushort Pulse10Count { get; set; }
    public uint IntervalMicroseconds { get; set; }
}

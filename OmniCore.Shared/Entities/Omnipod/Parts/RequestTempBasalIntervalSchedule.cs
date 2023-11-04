using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Framework.Omnipod.Parts;

public class RequestTempBasalIntervalSchedule : IMessagePart
{
    public required ushort LeadPulses10Count { get; set; }
    public required uint LeadPulseDelayMicroseconds { get; set; }
    public byte CurrentIntervalIndex { get; set; }
    public required PulseInterval[] PulseIntervals { get; set; }
    public bool BeepWhenSet { get; set; }
    public bool BeepWhenFinished { get; set; }

    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new RequestBasalIntervalSchedule
        {
            BeepWhenSet = span[0] == 0x80,
            CurrentIntervalIndex = span[1],
            LeadPulses10Count = span[2..].Read16(),
            LeadPulseDelayMicroseconds = span[4..].Read32(),
            PulseIntervals = GetPulseIntervals(span[8..])
        };
    }
    public int ToBytes(Span<byte> span)
    {
        if (BeepWhenSet)
            span[0] |= 0x80;
        if (BeepWhenFinished)
            span[0] |= 0x40;
        span[1] = CurrentIntervalIndex;
        span[2..].Write16(LeadPulses10Count);
        span[4..].Write32(LeadPulseDelayMicroseconds);

        int idx = 8;
        foreach (var interval in PulseIntervals)
        {
            span[idx..].Write16(interval.Pulse10Count);
            idx += 2;
            span[idx..].Write32(interval.IntervalMicroseconds);
            idx += 4;
        }
        return idx;
    }

    private static PulseInterval[] GetPulseIntervals(Span<byte> span)
    {
        var intervals = new List<PulseInterval>();
        while (span.Length > 0)
        {
            intervals.Add(new PulseInterval
            {
                Pulse10Count = span[0..].Read16(),
                IntervalMicroseconds = span[2..].Read32()
            });
            span = span[6..];
        }
        return intervals.ToArray();
    }


}
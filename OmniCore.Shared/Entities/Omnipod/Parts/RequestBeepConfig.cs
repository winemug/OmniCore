using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Shared.Entities.Omnipod.Parts;

public class RequestBeepConfig : IMessagePart
{
    public BeepType BeepNow { get; set; }
    public bool OnBasalStart { get; set; }
    public bool OnBasalEnd { get; set; }
    public int BasalBeepInterval { get; set; }
    public bool OnTempBasalStart { get; set; }
    public bool OnTempBasalEnd { get; set; }
    public int TempBasalBeepInterval { get; set; }
    public bool OnExtendedBolusStart { get; set; }
    public bool OnExtendedBolusEnd { get; set; }
    public int ExtendedBolusBeepInterval { get; set; }

    public static IMessagePart ToInstance(Span<byte> span)
    {
        var d0 = span.Read32();
        return new RequestBeepConfig
        {
            BeepNow = (BeepType)span.ReadBits(4, 4),
            OnBasalStart = span.ReadBits(8, 1) != 0,
            OnBasalEnd = span.ReadBits(9, 1) != 0,
            BasalBeepInterval = (int)span.ReadBits(10, 6),
            OnTempBasalStart = span.ReadBits(16, 1) != 0,
            OnTempBasalEnd = span.ReadBits(17, 1) != 0,
            TempBasalBeepInterval = (int)span.ReadBits(18, 6),
            OnExtendedBolusStart = span.ReadBits(24, 1) != 0,
            OnExtendedBolusEnd = span.ReadBits(25, 1) != 0,
            ExtendedBolusBeepInterval = (int)span.ReadBits(26, 6)
        };
    }

    public int ToBytes(Span<byte> span)
    {
        span.WriteBits((uint)BeepNow, 4, 4);
        span.WriteBits(OnBasalStart ? 1 : 0, 8, 1);
        span.WriteBits(OnBasalEnd ? 1: 0, 9, 1);
        span.WriteBits(BasalBeepInterval, 10, 6);
        span.WriteBits(OnTempBasalStart ? 1:0, 16, 1);
        span.WriteBits(OnTempBasalEnd ? 1:0, 17, 1);
        span.WriteBits(TempBasalBeepInterval, 18, 6);
        span.WriteBits(OnExtendedBolusStart ? 1:0, 24, 1);
        span.WriteBits(OnExtendedBolusEnd ? 1:0, 25, 1);
        span.WriteBits(ExtendedBolusBeepInterval, 26, 6);
        return 4;
    }
}
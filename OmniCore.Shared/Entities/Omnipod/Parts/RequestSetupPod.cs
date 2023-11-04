using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Framework.Omnipod.Parts;

public class RequestSetupPod : IMessagePart
{
    public int PacketTimeout { get; set; } = 50;
    public uint RadioAddress { get; set; }
    public uint Lot { get; set; }
    public uint Serial { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new RequestSetupPod
        {
            RadioAddress = span.Read32(),
            PacketTimeout = span[5],
            Date = new DateOnly(span[8] + 2000, span[6], span[7]),
            Time = new TimeOnly(span[9], span[10], 0),
            Lot = span[11..].Read32(),
            Serial = span[15..].Read32()
        };
    }

    public int ToBytes(Span<byte> span)
    {
        span.Write32(RadioAddress);
        span[5] = (byte)PacketTimeout;
        span[6] = (byte)(Date.Month);
        span[7] = (byte)(Date.Day);
        span[8] = (byte)(Date.Year - 2000);
        span[9] = (byte)Time.Hour;
        span[10] = (byte)Time.Minute;
        span[11..].Write32(Lot);
        span[15..].Write32(Serial);
        return 19;
    }
}
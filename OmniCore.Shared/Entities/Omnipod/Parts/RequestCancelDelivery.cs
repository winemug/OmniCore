using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Shared.Entities.Omnipod.Parts;

public class RequestCancelDelivery : IMessagePart
{
     public BeepType Beep { get; set; }
     // public bool StopExtendedBolus { get; set; }
     public bool StopBolus { get; set; }
     public bool StopTempBasal { get; set; }
     public bool StopBasal { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
        var b0 = span[0];
        return new RequestCancelDelivery
        {
            //StopExtendedBolus = (b0 & 0x08) > 0,
            StopBolus = (b0 & 0x04) > 0,
            StopTempBasal = (b0 & 0x02) > 0,
            StopBasal = (b0 & 0x01) > 0,
        };
    }

    public int ToBytes(Span<byte> span)
    {
        span.WriteBits((int)Beep, 0, 4);
        //span.WriteBits(StopExtendedBolus ? 1: 0,4, 1);
        span.WriteBits(StopBolus ? 1: 0,5, 1);
        span.WriteBits(StopTempBasal ? 1: 0,6, 1);
        span.WriteBits(StopBasal ? 1: 0,7, 1);
        return 1;
    }
}
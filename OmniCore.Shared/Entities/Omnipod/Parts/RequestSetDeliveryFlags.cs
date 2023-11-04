using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework.Omnipod.Parts;

public class RequestSetDeliveryFlags : IMessagePart
{
    public required byte VerificationFlag0 { get; set; }
    public required byte VerificationFlag1 { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new RequestSetDeliveryFlags
        {
            VerificationFlag0 = span[0],
            VerificationFlag1 = span[1]
        };
    }

    public int ToBytes(Span<byte> span)
    {
        span[0] = VerificationFlag0;
        span[1] = VerificationFlag1;
        return 2;
    }
}
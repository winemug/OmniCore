using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework.Omnipod.Parts;

public class RequestDeactivatePod : IMessagePart
{
    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new RequestDeactivatePod();
    }

    public int ToBytes(Span<byte> span)
    {
        return 0;
    }
}
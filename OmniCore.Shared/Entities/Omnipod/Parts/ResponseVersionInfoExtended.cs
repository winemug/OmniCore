using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework.Omnipod.Parts;

public class ResponseVersionInfoExtended : IMessagePart
{
    public static IMessagePart ToInstance(Span<byte> span)
    {
        throw new NotImplementedException();
    }

    public int ToBytes(Span<byte> span)
    {
        throw new NotImplementedException();
    }
}
using OmniCore.Shared.Extensions;

namespace OmniCore.Shared.Entities.Omnipod.Parts;

public class RequestAssignAddress : IMessagePart
{
    public uint Address { get; set; }

    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new RequestAssignAddress { Address = span[0..].Read32() };
    }

    public int ToBytes(Span<byte> span)
    {
        span[0..].Write32(Address);
        return 4;
    }
}
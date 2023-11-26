namespace OmniCore.Shared.Entities.Omnipod.Parts;

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
namespace OmniCore.Shared.Entities.Omnipod;

public interface IMessagePart
{
    int ToBytes(Span<byte> span);
}

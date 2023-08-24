using OmniCore.Common.Entities;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;

namespace OmniCore.Framework.Omnipod.Requests;

public class AssignAddressMessage : IMessageData
{
    public uint Address { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts => parts.MainPart.Type == PodMessagePartType.RequestAssignAddress;

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        Address = data.DWord(0);
        return this;
    }

    public IMessageParts ToParts()
    {
        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestAssignAddress,
                Data = new Bytes(Address),
                RequiresNonce = false
            });
    }
}
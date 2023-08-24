using OmniCore.Common.Entities;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;

namespace OmniCore.Framework.Omnipod.Requests;

public class GetStatusMessage : IMessageData
{
    public PodStatusType StatusType { get; set; }
    public static Predicate<IMessageParts> CanParse => parts => parts.MainPart.Type == PodMessagePartType.RequestStatus;

    public IMessageData FromParts(IMessageParts parts)
    {
        StatusType = (PodStatusType)parts.MainPart.Data[0];
        return this;
    }

    public IMessageParts ToParts()
    {
        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestStatus,
                RequiresNonce = false,
                Data = new Bytes((byte)StatusType)
            });
    }
}
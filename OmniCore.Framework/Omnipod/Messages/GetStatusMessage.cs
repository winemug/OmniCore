using OmniCore.Common.Pod;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Framework.Omnipod.Messages;

public class GetStatusMessage : IMessageData
{
    public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.RequestStatus;

    public PodStatusType StatusType { get; set; }

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
                Data = new Bytes((byte)StatusType),
            });
    }
}

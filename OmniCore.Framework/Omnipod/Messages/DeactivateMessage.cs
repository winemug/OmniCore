using OmniCore.Common.Pod;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Framework.Omnipod.Messages;

public class DeactivateMessage : IMessageData
{
    public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.RequestDeactivatePod;

    public IMessageData FromParts(IMessageParts parts)
    {
        return this;
    }

    public IMessageParts ToParts()
    {
        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestDeactivatePod,
                RequiresNonce = true,
                Data = new Bytes(),
            });
    }
}

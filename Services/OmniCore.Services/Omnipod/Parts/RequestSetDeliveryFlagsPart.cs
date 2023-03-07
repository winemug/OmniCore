using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestSetDeliveryFlagsPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override PodMessageType Type => PodMessageType.RequestSetDeliveryFlags;
}
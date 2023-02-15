namespace OmniCore.Services;

public class RequestSetDeliveryFlagsPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override RadioMessageType Type => RadioMessageType.RequestSetDeliveryFlags;
}
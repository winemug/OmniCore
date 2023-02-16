namespace OmniCore.Services;

public class RequestBasalPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestBasal;
}
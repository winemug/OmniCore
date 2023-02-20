namespace OmniCore.Services;

public class RequestBasalPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestBasal;
}
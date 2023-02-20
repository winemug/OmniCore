namespace OmniCore.Services;

public class RequestTempBasalPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestTempBasal;
}
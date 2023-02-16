namespace OmniCore.Services;

public class RequestTempBasalPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestTempBasal;
}
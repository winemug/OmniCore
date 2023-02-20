namespace OmniCore.Services;

public class RequestBolusPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestBolus;
}
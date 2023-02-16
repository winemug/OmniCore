namespace OmniCore.Services;

public class RequestBolusPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestBolus;
}
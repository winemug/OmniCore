namespace OmniCore.Services;

public class RequestDeactivatePodPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override RadioMessageType Type { get; }
}
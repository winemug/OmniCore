namespace OmniCore.Services;

public class RequestSetupPodPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestSetupPod;
}
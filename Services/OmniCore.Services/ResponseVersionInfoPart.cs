namespace OmniCore.Services;

public class ResponseVersionInfoPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.ResponseVersionInfo;
}
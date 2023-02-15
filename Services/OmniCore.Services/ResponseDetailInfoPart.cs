namespace OmniCore.Services;

public class ResponseDetailInfoPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.ResponseDetailInfo;
}
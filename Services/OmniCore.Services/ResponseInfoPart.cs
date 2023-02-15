using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public abstract class ResponseInfoPart : MessagePart
{
    public abstract RequestStatusType StatusType { get; } 
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.ResponseDetailInfo;

}
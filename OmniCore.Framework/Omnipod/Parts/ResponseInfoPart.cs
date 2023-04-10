using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public abstract class ResponseInfoPart : MessagePart
{
    public abstract RequestStatusType StatusType { get; }
    public override bool RequiresNonce => false;
    public override PodMessagePartType Type => PodMessagePartType.ResponseInfo;
}
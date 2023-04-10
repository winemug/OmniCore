using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestDeactivatePodPart : MessagePart
{
    public RequestDeactivatePodPart()
    {
        Data = new Bytes();
    }

    public override bool RequiresNonce => true;
    public override PodMessagePartType Type => PodMessagePartType.RequestDeactivatePod;
}
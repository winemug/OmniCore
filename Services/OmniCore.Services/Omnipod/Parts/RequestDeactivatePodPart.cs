using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Definitions;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services;

public class RequestDeactivatePodPart : MessagePart
{
    public RequestDeactivatePodPart()
    {
        Data = new Bytes();
    }

    public override bool RequiresNonce => true;
    public override PodMessageType Type => PodMessageType.RequestDeactivatePod;
}
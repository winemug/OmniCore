using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestBolusPart : MessagePart
{
    public RequestBolusPart(BolusEntry be)
    {
    }

    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestBolus;
}
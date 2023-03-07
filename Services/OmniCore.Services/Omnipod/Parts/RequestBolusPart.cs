using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestBolusPart : MessagePart
{
    public RequestBolusPart(BolusEntry be)
    {
    }

    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestBolus;
}
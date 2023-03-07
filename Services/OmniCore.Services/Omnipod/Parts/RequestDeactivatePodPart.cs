using OmniCore.Services.Interfaces;

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
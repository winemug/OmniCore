using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestDeactivatePodPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override PodMessageType Type => PodMessageType.RequestDeactivatePod;

    public RequestDeactivatePodPart()
    {
        Data = new Bytes();
    }
}
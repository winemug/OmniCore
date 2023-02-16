using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestDeactivatePodPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override RadioMessageType Type => RadioMessageType.RequestDeactivatePod;

    public RequestDeactivatePodPart()
    {
        Data = new Bytes();
    }
}
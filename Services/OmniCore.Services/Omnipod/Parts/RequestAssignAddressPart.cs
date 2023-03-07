using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestAssignAddressPart : MessagePart
{
    public RequestAssignAddressPart(uint radioAddress)
    {
        Data = new Bytes(radioAddress);
    }

    public override bool RequiresNonce => false;

    public override PodMessageType Type => PodMessageType.RequestAssignAddress;
}
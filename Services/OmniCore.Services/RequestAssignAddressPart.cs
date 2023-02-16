using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestAssignAddressPart : MessagePart
{
    public override bool RequiresNonce => false;

    public override RadioMessageType Type => RadioMessageType.RequestAssignAddress;

    public RequestAssignAddressPart(uint radioAddress)
    {
        Data = new Bytes(radioAddress);
    }
}
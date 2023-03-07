using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Definitions;
using OmniCore.Services.Interfaces.Entities;

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
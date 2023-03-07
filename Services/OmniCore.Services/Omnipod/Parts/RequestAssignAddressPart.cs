using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

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
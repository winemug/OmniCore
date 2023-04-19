using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestSetDeliveryFlagsPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override PodMessagePartType Type => PodMessagePartType.RequestSetDeliveryFlags;

    public RequestSetDeliveryFlagsPart(byte b16, byte b17)
    {
        Data = new Bytes(b16).Append(b17);
    }
}
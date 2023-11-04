using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework.Omnipod.Parts;

public class RequestStatus : IMessagePart
{
    public required PodStatusType StatusType { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new RequestStatus
        {
            StatusType = (PodStatusType)span[0]
        };
    }

    public int ToBytes(Span<byte> span)
    {
        span[0] = (byte)StatusType;
        return 1;
    }
}
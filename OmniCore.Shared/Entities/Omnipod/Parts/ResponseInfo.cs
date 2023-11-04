using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework.Omnipod.Parts;

public class ResponseInfo : IMessagePart
{
    public required PodStatusType StatusType { get; set; }
    public required byte[] StatusData { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new ResponseInfo
        {
            StatusType = (PodStatusType)span[0],
            StatusData = span[1..].ToArray()
        };
    }

    public int ToBytes(Span<byte> span)
    {
        span[0] = (byte)StatusType;
        StatusData.CopyTo(span[1..]);
        return StatusData.Length + 1;
    }
}
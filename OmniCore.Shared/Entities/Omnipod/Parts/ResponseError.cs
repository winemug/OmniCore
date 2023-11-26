namespace OmniCore.Shared.Entities.Omnipod.Parts;

public class ResponseError : IMessagePart
{
     public required byte Code { get; set; }
     public required byte[] Data { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new ResponseError
        {
            Code = span[0],
            Data = span[1..].ToArray()
        };
    }

    public int ToBytes(Span<byte> span)
    {
        span[0] = Code;
        Data.CopyTo(span[1..]);
        return Data.Length + 1;
    }
}
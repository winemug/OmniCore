using OmniCore.Common.Pod;

namespace OmniCore.Framework.Omnipod.Responses;

public class ErrorMessage : IMessageData
{
    public byte Code { get; set; }
    public ushort AdditionalData { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts =>
            parts.MainPart.Type == PodMessagePartType.ResponseError &&
            parts.MainPart.Data[0] != 0x14;

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        Code = data[0];
        AdditionalData = data.Word(1);
        return this;
    }

    public IMessageParts ToParts()
    {
        throw new NotImplementedException();
    }
}
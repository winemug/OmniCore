using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseErrorPart : MessagePart
{
    public ResponseErrorPart(Bytes data)
    {
        Data = data;
        ErrorCode = data[0];
        ErrorValue = data.Word(1);
    }

    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.ResponseError;

    public byte ErrorCode { get; set; }
    public ushort ErrorValue { get; set; }
}
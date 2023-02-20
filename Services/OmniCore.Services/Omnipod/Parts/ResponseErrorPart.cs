using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseErrorPart : MessagePart
{ 
    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.ResponseError;

    public byte ErrorCode { get; set; }
    public ushort ErrorValue { get; set; }
    public ResponseErrorPart(Bytes data)
    {
        ErrorCode = data[0];
        ErrorValue = data.Word(1);
    }
}
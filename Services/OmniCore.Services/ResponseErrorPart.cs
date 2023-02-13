using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseErrorPart : RadioMessagePart
{ 
    public override RadioMessageType Type => RadioMessageType.ResponseError;

    public byte ErrorCode { get; set; }
    public ushort ErrorValue { get; set; }
    public ResponseErrorPart(Bytes data)
    {
        ErrorCode = data[0];
        ErrorValue = data.Word(1);
    }
}
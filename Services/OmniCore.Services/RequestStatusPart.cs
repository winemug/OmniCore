using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestStatusPart : RadioMessagePart
{
    public override RadioMessageType Type => RadioMessageType.RequestStatus;

    public RequestStatusPart(RequestStatusType type)
    {
        Data = new Bytes((byte)type);
    }
}

public enum RequestStatusType
{
    Default = 0,
    Extended = 1
}
using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestStatusPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestStatus;

    public RequestStatusPart(RequestStatusType type)
    {
        Data = new Bytes((byte)type);
    }
}

public enum RequestStatusType
{
    Default = 0,
    Alerts = 1,
    Extended = 2,
    PulseLogRecent = 3,
    Activation = 5,
    PulseLogLast = 80,
    PulseLogPrev = 81,
}
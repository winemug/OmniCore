using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestStatusPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestStatus;

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
    PulseLogPrevious = 81,
}
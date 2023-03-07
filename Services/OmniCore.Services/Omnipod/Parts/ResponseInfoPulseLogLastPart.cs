using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoPulseLogLastPart : ResponseInfoPart
{
    public ResponseInfoPulseLogLastPart(Bytes data)
    {
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.PulseLogLast;
}
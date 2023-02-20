using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoPulseLogLastPart : ResponseInfoPart
{
    public override RequestStatusType StatusType => RequestStatusType.PulseLogLast;

    public ResponseInfoPulseLogLastPart(Bytes data)
    {
        
    }
}
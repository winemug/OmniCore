using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoPulseLogPreviousPart : ResponseInfoPart
{
    public override RequestStatusType StatusType => RequestStatusType.PulseLogPrevious;

    public ResponseInfoPulseLogPreviousPart(Bytes data)
    {
        
    }
}
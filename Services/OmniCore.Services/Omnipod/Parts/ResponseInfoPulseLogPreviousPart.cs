using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoPulseLogPreviousPart : ResponseInfoPart
{
    public ResponseInfoPulseLogPreviousPart(Bytes data)
    {
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.PulseLogPrevious;
}
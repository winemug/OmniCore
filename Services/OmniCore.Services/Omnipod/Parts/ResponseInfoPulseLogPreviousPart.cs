using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services;

public class ResponseInfoPulseLogPreviousPart : ResponseInfoPart
{
    public ResponseInfoPulseLogPreviousPart(Bytes data)
    {
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.PulseLogPrevious;
}
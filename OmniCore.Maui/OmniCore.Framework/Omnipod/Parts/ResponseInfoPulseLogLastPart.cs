using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services;

public class ResponseInfoPulseLogLastPart : ResponseInfoPart
{
    public ResponseInfoPulseLogLastPart(Bytes data)
    {
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.PulseLogLast;
}
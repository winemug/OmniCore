using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services;

public class ResponseInfoPulseLogRecentPart : ResponseInfoPart
{
    public ResponseInfoPulseLogRecentPart(Bytes data)
    {
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.PulseLogRecent;
}
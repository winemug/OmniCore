using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoPulseLogRecentPart : ResponseInfoPart
{
    public ResponseInfoPulseLogRecentPart(Bytes data)
    {
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.PulseLogRecent;
}
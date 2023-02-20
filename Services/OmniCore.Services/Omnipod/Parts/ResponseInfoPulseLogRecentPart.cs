using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoPulseLogRecentPart : ResponseInfoPart
{
    public override RequestStatusType StatusType => RequestStatusType.PulseLogRecent;
    
    public ResponseInfoPulseLogRecentPart(Bytes data)
    {
    }
}
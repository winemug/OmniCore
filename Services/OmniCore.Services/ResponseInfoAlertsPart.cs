using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoAlertsPart : ResponseInfoPart
{
    public override RequestStatusType StatusType => RequestStatusType.Alerts;

    public ResponseInfoAlertsPart(Bytes data)
    {
    }
}
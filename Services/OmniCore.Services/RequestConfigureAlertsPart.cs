namespace OmniCore.Services;

public class RequestConfigureAlertsPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override RadioMessageType Type => RadioMessageType.RequestConfigureAlerts;
}
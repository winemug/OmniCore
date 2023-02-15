namespace OmniCore.Services;

public class RequestAcknowledgeAlertsPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override RadioMessageType Type => RadioMessageType.RequestAcknowledgeAlerts;
}
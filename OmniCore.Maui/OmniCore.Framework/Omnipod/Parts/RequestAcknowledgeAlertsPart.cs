using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestAcknowledgeAlertsPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override PodMessageType Type => PodMessageType.RequestAcknowledgeAlerts;
}
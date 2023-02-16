using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestAcknowledgeAlerts : MessagePart
{
    public override bool RequiresNonce => true;
    public override RadioMessageType Type => RadioMessageType.RequestAcknowledgeAlerts;

}
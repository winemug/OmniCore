using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestAcknowledgeAlertsPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override PodMessagePartType Type => PodMessagePartType.RequestAcknowledgeAlerts;

    public RequestAcknowledgeAlertsPart()
    {
        Data = new Bytes((byte)0xff);
    }
}
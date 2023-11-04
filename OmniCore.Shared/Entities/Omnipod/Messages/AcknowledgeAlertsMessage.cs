using OmniCore.Framework.Omnipod.Parts;
using OmniCore.Shared;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework.Omnipod.Messages;

public class AcknowledgeAlertsMessage
{
    private RequestAcknowledgeAlerts Part;
    public bool[] AlertIndices => Part.AlertIndices;
    public AcknowledgeAlertsMessage(bool[] indices)
    {
        Part = new RequestAcknowledgeAlerts
        {
            AlertIndices = indices
        };
    }
}
using OmniCore.Shared.Entities.Omnipod.Parts;

namespace OmniCore.Shared.Entities.Omnipod.Messages;

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
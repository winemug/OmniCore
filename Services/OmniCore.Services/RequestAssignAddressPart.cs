namespace OmniCore.Services;

public class RequestAssignAddressPart : MessagePart
{
    public override bool RequiresNonce => false;

    public override RadioMessageType Type => RadioMessageType.RequestAssignAddress;
}
namespace OmniCore.Services;

public class RequestInsulinSchedulePart : MessagePart
{
    public override bool RequiresNonce => true;
    public override RadioMessageType Type => RadioMessageType.RequestInsulinSchedule;
}
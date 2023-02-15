namespace OmniCore.Services;

public class RequestBasalSchedulePart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestBasalSchedule;
}
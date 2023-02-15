namespace OmniCore.Services;

public class RequestTempBasalSchedulePart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestTempBasalSchedule;
}
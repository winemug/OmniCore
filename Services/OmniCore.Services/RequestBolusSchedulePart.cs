namespace OmniCore.Services;

public class RequestBolusSchedulePart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.RequestBolusSchedule;
}
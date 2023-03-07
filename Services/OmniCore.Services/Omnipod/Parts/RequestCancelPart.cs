using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestCancelPart : MessagePart
{
    public RequestCancelPart(
        BeepType beep,
        bool cancelBolus,
        bool cancelTempBasal,
        bool cancelBasal)
    {
        var b0 = (int)beep << 4;
        b0 |= cancelBolus ? 0x04 : 0x00;
        b0 |= cancelTempBasal ? 0x02 : 0x00;
        b0 |= cancelBasal ? 0x01 : 0x00;

        Data = new Bytes((byte)b0);
    }

    public override bool RequiresNonce => true;
    public override PodMessageType Type => PodMessageType.RequestCancelDelivery;
}
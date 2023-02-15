using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestCancelPart : MessagePart
{
    public override bool RequiresNonce => true;
    public override RadioMessageType Type => RadioMessageType.RequestCancelDelivery;

    public RequestCancelPart(
        BeepType beep,
        bool cancelBolus,
        bool cancelTempBasal,
        bool cancelBasal)
    {
        int b0 = ((int)beep) << 4;
        b0 |= cancelBolus ? 0x04 : 0x00;
        b0 |= cancelTempBasal ? 0x02 : 0x00;
        b0 |= cancelBasal ? 0x01 : 0x00;
        
        Data = new Bytes((byte)b0);
    }
}
using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestCancelPart : RadioMessagePart
{
    public override RadioMessageType Type => RadioMessageType.RequestCancelDelivery;

    public RequestCancelPart(uint nonce,
        BeepType beep,
        bool cancelBolus,
        bool cancelTempBasal,
        bool cancelBasal)
    {
        Nonce = nonce;
        int b0 = ((int)beep) << 4;
        b0 |= cancelBolus ? 0x04 : 0x00;
        b0 |= cancelTempBasal ? 0x02 : 0x00;
        b0 |= cancelBasal ? 0x01 : 0x00;
        
        Data = new Bytes((byte)b0);
    }
}
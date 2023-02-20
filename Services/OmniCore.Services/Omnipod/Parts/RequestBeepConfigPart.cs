using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestBeepConfigPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestBeepConfig;

    public RequestBeepConfigPart(BeepType beepNow,
        bool onBasalStart, bool onBasalEnd, int basalBeepInterval,
        bool onTempBasalStart, bool onTempBasalEnd, int tempBasalBeepInterval,
        bool onExtendedBolusStart, bool onExtendedBolusEnd, int extendedBolusBeepInterval)
    {
        int d0 = 0;
        d0 |= onBasalStart ? 0x00800000 : 0x00000000;
        d0 |= onBasalEnd ? 0x00400000 : 0x00000000;
        d0 |= (basalBeepInterval & 0b00111111) << 16;
        
        d0 |= onTempBasalStart ? 0x00008000 : 0x00000000;
        d0 |= onTempBasalEnd ? 0x00004000 : 0x00000000;
        d0 |= (tempBasalBeepInterval & 0b00111111) << 8;
        
        d0 |= onExtendedBolusStart ? 0x00000080 : 0x00000000;
        d0 |= onExtendedBolusEnd ? 0x00000040 : 0x00000000;
        d0 |= (extendedBolusBeepInterval & 0b00111111);

        d0 |= ((int)beepNow & 0x0F) << 24;
        Data = new Bytes((uint)d0);
    }
}

public enum BeepType
{
    NoSound=0x00, 
    Beep4x=0x01,
    BipBeep4x=0x02,
    BipBip=0x03,
    Beep=0x04,
    Beep3x=0x05,
    Beeeeeep=0x06,
    BipBipBip2x=0x07,
    Beeep2x=0x08,
    BeepBeep=0x0b,
    Beeep=0x0c,
    BipBeeeeep=0x0d,
    Continuous5seconds=0x0e,
    Silent=0x0f 
}

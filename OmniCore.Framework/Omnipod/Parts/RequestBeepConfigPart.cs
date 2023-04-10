using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestBeepConfigPart : MessagePart
{
    public RequestBeepConfigPart(BeepType beepNow,
        bool onBasalStart, bool onBasalEnd, int basalBeepInterval,
        bool onTempBasalStart, bool onTempBasalEnd, int tempBasalBeepInterval,
        bool onExtendedBolusStart, bool onExtendedBolusEnd, int extendedBolusBeepInterval)
    {
        var d0 = 0;
        d0 |= onBasalStart ? 0x00800000 : 0x00000000;
        d0 |= onBasalEnd ? 0x00400000 : 0x00000000;
        d0 |= (basalBeepInterval & 0b00111111) << 16;

        d0 |= onTempBasalStart ? 0x00008000 : 0x00000000;
        d0 |= onTempBasalEnd ? 0x00004000 : 0x00000000;
        d0 |= (tempBasalBeepInterval & 0b00111111) << 8;

        d0 |= onExtendedBolusStart ? 0x00000080 : 0x00000000;
        d0 |= onExtendedBolusEnd ? 0x00000040 : 0x00000000;
        d0 |= extendedBolusBeepInterval & 0b00111111;

        d0 |= ((int)beepNow & 0x0F) << 24;
        Data = new Bytes((uint)d0);
    }

    public override bool RequiresNonce => false;
    public override PodMessagePartType Type => PodMessagePartType.RequestBeepConfig;
}
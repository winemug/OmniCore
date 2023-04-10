using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestBolusPart : MessagePart
{
    public RequestBolusPart(BolusEntry be)
    {
        var data = new Bytes();
        data.Append(0);
        ushort totalPulses10 = (ushort)(be.ImmediatePulseCount * 10);
        uint pulseInterval = (uint)(be.ImmediatePulseInterval125ms * 100000 / 8);
        Data = data.Append(totalPulses10).Append(pulseInterval).Append((ushort)0).Append((uint)0);
    }

    public override bool RequiresNonce => false;
    public override PodMessagePartType Type => PodMessagePartType.RequestBolus;
}
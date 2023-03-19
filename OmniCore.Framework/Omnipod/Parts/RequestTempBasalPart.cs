using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestTempBasalPart : MessagePart
{
    public RequestTempBasalPart(BasalRateEntry bre)
    {
        var data = new Bytes();
        data.Append(0).Append(0);

        var totalPulses10 = bre.PulsesPerHour * bre.HalfHourCount * 10 / 2;
        var hhPulses10 = bre.PulsesPerHour * 10 / 2;

        var avgPulseIntervalMs = 1800000000;
        if (bre.PulsesPerHour > 0)
            avgPulseIntervalMs = 360000000 / bre.PulsesPerHour;

        var pulseRecord = new Bytes();
        if (totalPulses10 == 0)
        {
            for (int i = 0; i < bre.HalfHourCount; i++)
                pulseRecord.Append((ushort)0).Append((uint)avgPulseIntervalMs);
        }
        else
        {
            var pulses10remaining = totalPulses10;
            while (pulses10remaining > 0)
            {
                var pulses10record = pulses10remaining;
                if (pulses10remaining > 0xFFFF)
                {
                    if (hhPulses10 > 0xFFFF)
                        pulses10record = 0XFFFF;
                    else
                    {
                        var hhCountFitting = 0xFFFF / hhPulses10;
                        if (hhCountFitting % 2 == 0)
                            hhCountFitting--;
                        pulses10record = hhCountFitting * hhPulses10;
                    }
                }
                pulseRecord.Append((ushort)pulses10record).Append((uint)avgPulseIntervalMs);
                pulses10remaining -= pulses10record;
            }
        }


        Data = data.Append(pulseRecord.Sub(0, 6)).Append(pulseRecord);
    }

    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestTempBasal;
}
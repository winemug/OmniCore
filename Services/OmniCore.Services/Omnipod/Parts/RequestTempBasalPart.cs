using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class RequestTempBasalPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestTempBasal;

    public RequestTempBasalPart(BasalRateEntry bre)
    {
        var data = new Bytes();
        data.Append((byte)0).Append((byte)0);

        int totalPulses10 = bre.PulsesPerHour * bre.HalfHourCount * 10 / 2;

        int avgPulseIntervalMs = 1800000000;
        if (bre.PulsesPerHour > 0)
            avgPulseIntervalMs = (3600 * 1000) / bre.PulsesPerHour;

        var pulses10remaining = totalPulses10;
        var pulseRecord = new Bytes();
        while (pulses10remaining > 0)
        {
            var pulses10record = 0;
            if (pulses10remaining > 0xFFFF)
            {
                
            }
            else
            {
                pulses10record = pulses10remaining;
            }
            pulseRecord.Append((ushort)pulses10record).Append((uint)avgPulseIntervalMs);
            pulses10remaining -= pulses10record;
        }
        
       
        Data = data;
    }
}
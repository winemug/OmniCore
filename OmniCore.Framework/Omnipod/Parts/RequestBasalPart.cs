using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestBasalPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.RequestBasal;

    public RequestBasalPart(BasalRateEntry[] basalRateEntries, TimeOnly podTime)
    {
        var hhPulses = DistributeHourlyRatesToHalfHourPulses(basalRateEntries);
        var schedules = GetConsecutiveSchedules(hhPulses);
        
        var data = new Bytes();
        data.Append(0).Append(0);

        
        //var hhPulses10 = bre.PulsesPerHour * 10 / 2;

        // var avgPulseIntervalMs = 1800000000;
        // if (bre.PulsesPerHour > 0)
        //     avgPulseIntervalMs = 360000000 / bre.PulsesPerHour;

        var pulseRecord = new Bytes();
        int currentHH = 0;
        foreach (var schedule in schedules)
        {
            var pulses10remaining = schedule.PulsesPerBlock * schedule.BlockCount
                + (schedule.AddAlternatingExtraPulse ? (schedule.BlockCount / 2) + (currentHH % 2) : 0 );
            currentHH += schedule.BlockCount;
            var hhPulses10 = schedule.PulsesPerBlock * 10 + (schedule.AddAlternatingExtraPulse ? 5 : 0);
            var avgPulseIntervalMs = 360000000 / (schedule.PulsesPerBlock * 2 + (schedule.AddAlternatingExtraPulse ? 1:0));
            var pulses10record = pulses10remaining;
            while (pulses10remaining > 0)
            {
                if (pulses10remaining > 0xFFFF)
                {
                    if (hhPulses10 > 0xFFFF)
                        pulses10record = 0XFFFF;
                    else
                    {
                        var hhCountFitting = 0xFFFF / hhPulses10;
                        if (hhCountFitting % 2 + currentHH % 2 == 0)
                            hhCountFitting--;
                        pulses10record = hhCountFitting * hhPulses10;
                    }
                }
                pulseRecord.Append((ushort)pulses10record).Append((uint)avgPulseIntervalMs);
                pulses10remaining -= pulses10record;
            }
        }

        var currentHalfHour = podTime.Hour * 2;
        if (podTime.Minute >= 30)
        {
            currentHalfHour++;
        }

        var podTimeMs = podTime.Ticks * 1000 / 10;
        var halfHourMs = 30 * 60 * 1000;
        var spentCurrentHHMs = podTimeMs % halfHourMs;
        var toNextHHMs = halfHourMs - spentCurrentHHMs;

        var currentHHPulses10 = hhPulses[currentHalfHour]*10;
        var remainingHHPulses10 = currentHHPulses10 - (currentHHPulses10 * spentCurrentHHMs / halfHourMs);

        var avgPulseIntervalCurrentMs = 360000000 / hhPulses[currentHalfHour];
        Data = data.Append((ushort)remainingHHPulses10).Append((uint)avgPulseIntervalCurrentMs).Append(pulseRecord);

    }
    
        private InsulinSchedule[] GetConsecutiveSchedules(int[] hhPulses)
    {
        List<InsulinSchedule> schedules = new();

        var schedule = new InsulinSchedule
        {
            BlockCount = 0,
            AddAlternatingExtraPulse = false,
            PulsesPerBlock = hhPulses[0]
        };

        int currentBlock = 0;
        foreach (var hhPulse in hhPulses)
        {
            var oldBlockCount = schedule.BlockCount;
            var oldPulsesPerBlock = schedule.PulsesPerBlock;
            var oldAlternatingMode = schedule.AddAlternatingExtraPulse;
            var addNewSchedule = false;
            if (schedule.BlockCount < 2)
            {
                if (schedule.PulsesPerBlock == hhPulse)
                {
                    schedule = new InsulinSchedule
                    {
                        BlockCount = oldBlockCount + 1,
                        AddAlternatingExtraPulse = false,
                        PulsesPerBlock = oldPulsesPerBlock
                    };
                }
                else if (schedule.PulsesPerBlock == hhPulse - currentBlock % 2)
                {
                    schedule = new InsulinSchedule
                    {
                        BlockCount = oldBlockCount + 1,
                        AddAlternatingExtraPulse = true,
                        PulsesPerBlock = oldPulsesPerBlock
                    };
                }
                else
                {
                    addNewSchedule = true;
                }
            }
            else
            {
                var hhPulsesCompare = oldAlternatingMode ? hhPulse - currentBlock % 2 : hhPulse;
                if (schedule.PulsesPerBlock == hhPulsesCompare)
                {
                    schedule = new InsulinSchedule
                    {
                        BlockCount = oldBlockCount + 1,
                        AddAlternatingExtraPulse = oldAlternatingMode,
                        PulsesPerBlock = oldPulsesPerBlock
                    };
                }
                else
                {
                    addNewSchedule = true;
                }
            }

            if (addNewSchedule)
            {
                schedules.Add(schedule);
                schedule = new InsulinSchedule
                {
                    BlockCount = 1,
                    AddAlternatingExtraPulse = false,
                    PulsesPerBlock = hhPulse
                };
            }
            currentBlock++;
        }
        schedules.Add(schedule);
        return schedules.ToArray();
    }
        private int[] DistributeHourlyRatesToHalfHourPulses(BasalRateEntry[] entries)
        {
            var hourlyRatePer30Mins = new int[48];
            var halfHourCount = 0;
            foreach (var entry in entries)
                for (var i = 0; i < entry.HalfHourCount; i++)
                {
                    hourlyRatePer30Mins[halfHourCount] = entry.PulsesPerHour;
                    halfHourCount++;
                }

            var pulsesPerHalfHour = new int[halfHourCount];
            var overflow = false;
            for (var i = 0; i < halfHourCount; i++)
            {
                pulsesPerHalfHour[i] = hourlyRatePer30Mins[i] / 2;
                if (hourlyRatePer30Mins[i] % 2 == 1)
                {
                    if (overflow)
                        pulsesPerHalfHour[i] += 1;
                    overflow = !overflow;
                }
            }

            return pulsesPerHalfHour;
        }
}
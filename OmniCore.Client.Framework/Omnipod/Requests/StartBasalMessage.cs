using OmniCore.Common.Entities;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;

namespace OmniCore.Framework.Omnipod.Requests;

public class StartBasalMessage : IMessageData
{
    public StartBasalMessage()
    {
        PulsesPerHour48HalfHours = new int[48];
    }

    public TimeOnly PodTime { get; set; }
    public int[] PulsesPerHour48HalfHours { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts => parts.MainPart.Type == PodMessagePartType.RequestBasal &&
                 parts.SubPart?.Type == PodMessagePartType.RequestInsulinSchedule;

    public IMessageData FromParts(IMessageParts parts)
    {
        return this;
    }

    public IMessageParts ToParts()
    {
        if (PulsesPerHour48HalfHours.Length != 48)
            throw new ApplicationException("You must set 48 half hour entries");

        if (PulsesPerHour48HalfHours.Where(p => p < 1).Any())
            throw new ApplicationException("Each half hour entry must be at least 1 pulse/hour");

        var hhPulses = new int[48];
        var overflow = false;
        for (var i = 0; i < 48; i++)
        {
            hhPulses[i] = PulsesPerHour48HalfHours[i] / 2;
            if (PulsesPerHour48HalfHours[i] % 2 == 1)
            {
                if (overflow)
                    hhPulses[i] += 1;
                overflow = !overflow;
            }
        }

        var schedules = ScheduleHelper.GetConsecutiveSchedules(hhPulses);

        var mainData = new Bytes();
        mainData.Append(0).Append(0);

        var pulseRecord = new Bytes();
        var currentHh = 0;
        var hhAvgPulses10 = new int[48];

        foreach (var schedule in schedules)
        {
            var totalPulses10Remaining = 10 * schedule.PulsesPerBlock * schedule.BlockCount
                                         + (schedule.AddAlternatingExtraPulse ? 10 : 0) *
                                         (schedule.BlockCount / 2 + currentHh % 2);

            var hhPulses10 = totalPulses10Remaining / schedule.BlockCount;

            var avgPulseIntervalMs = 1800 * 1000 * 1000 / hhPulses10;
            var pulses10Record = totalPulses10Remaining;
            while (totalPulses10Remaining > 0)
            {
                if (totalPulses10Remaining > 0xFFFF)
                {
                    if (hhPulses10 > 0xFFFF)
                    {
                        pulses10Record = 0XFFFF;
                    }
                    else
                    {
                        var hhCountFitting = 0xFFFF / hhPulses10;
                        if (hhCountFitting % 2 + currentHh % 2 == 0)
                            hhCountFitting--;
                        pulses10Record = hhCountFitting * hhPulses10;
                    }
                }

                pulseRecord.Append((ushort)pulses10Record).Append((uint)avgPulseIntervalMs);
                totalPulses10Remaining -= pulses10Record;
            }

            for (var i = currentHh; i < currentHh + schedule.BlockCount; i++) hhAvgPulses10[i] = hhPulses10;

            currentHh += schedule.BlockCount;
        }

        var currentHalfHour = PodTime.Hour * 2;
        if (PodTime.Minute >= 30) currentHalfHour++;

        var podTimeMs = PodTime.Ticks / 10;
        var halfHourMs = 1800 * 1000 * 1000;
        var spentCurrentHhMs = podTimeMs % halfHourMs;
        var toNextHhMs = halfHourMs - spentCurrentHhMs;

        var currentHhPulses10 = hhAvgPulses10[currentHalfHour];
        var remainingHhPulses10 = currentHhPulses10 - currentHhPulses10 * spentCurrentHhMs / halfHourMs;

        var avgPulseIntervalCurrentMs = 1800 * 1000 * 1000 / currentHhPulses10;
        mainData.Append((ushort)remainingHhPulses10).Append((uint)avgPulseIntervalCurrentMs).Append(pulseRecord);

        var podTime125Ms = PodTime.Ticks / 10 / 1000 / 125;
        var halfHour125Ms = 30 * 60 * 1000 / 125;
        var spentCurrentHh125Ms = podTime125Ms % halfHour125Ms;
        var toNextHh125Ms = halfHour125Ms - spentCurrentHh125Ms;

        var currentHhPulses = hhPulses[currentHalfHour];
        var remainingHhPulses = currentHhPulses - currentHhPulses * spentCurrentHh125Ms / halfHour125Ms;

        var scheduleData = ScheduleHelper.GetScheduleDataWithChecksum(
            (byte)currentHalfHour,
            (ushort)toNextHh125Ms,
            (ushort)remainingHhPulses,
            schedules);

        var subData = new Bytes(0).Append(scheduleData);


        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestBasal,
                RequiresNonce = false,
                Data = mainData
            },
            new MessagePart
            {
                Type = PodMessagePartType.RequestInsulinSchedule,
                RequiresNonce = true,
                Data = subData
            });
    }
}
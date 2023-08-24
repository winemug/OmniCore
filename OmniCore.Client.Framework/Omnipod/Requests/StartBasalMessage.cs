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
        var currentHH = 0;
        var hhAvgPulses10 = new int[48];

        foreach (var schedule in schedules)
        {
            var totalPulses10remaining = 10 * schedule.PulsesPerBlock * schedule.BlockCount
                                         + (schedule.AddAlternatingExtraPulse ? 10 : 0) *
                                         (schedule.BlockCount / 2 + currentHH % 2);

            var hhPulses10 = totalPulses10remaining / schedule.BlockCount;

            var avgPulseIntervalMs = 1800 * 1000 * 1000 / hhPulses10;
            var pulses10record = totalPulses10remaining;
            while (totalPulses10remaining > 0)
            {
                if (totalPulses10remaining > 0xFFFF)
                {
                    if (hhPulses10 > 0xFFFF)
                    {
                        pulses10record = 0XFFFF;
                    }
                    else
                    {
                        var hhCountFitting = 0xFFFF / hhPulses10;
                        if (hhCountFitting % 2 + currentHH % 2 == 0)
                            hhCountFitting--;
                        pulses10record = hhCountFitting * hhPulses10;
                    }
                }

                pulseRecord.Append((ushort)pulses10record).Append((uint)avgPulseIntervalMs);
                totalPulses10remaining -= pulses10record;
            }

            for (var i = currentHH; i < currentHH + schedule.BlockCount; i++) hhAvgPulses10[i] = hhPulses10;

            currentHH += schedule.BlockCount;
        }

        var currentHalfHour = PodTime.Hour * 2;
        if (PodTime.Minute >= 30) currentHalfHour++;

        var podTimeMs = PodTime.Ticks / 10;
        var halfHourMs = 1800 * 1000 * 1000;
        var spentCurrentHHMs = podTimeMs % halfHourMs;
        var toNextHHMs = halfHourMs - spentCurrentHHMs;

        var currentHHPulses10 = hhAvgPulses10[currentHalfHour];
        var remainingHHPulses10 = currentHHPulses10 - currentHHPulses10 * spentCurrentHHMs / halfHourMs;

        var avgPulseIntervalCurrentMs = 1800 * 1000 * 1000 / currentHHPulses10;
        mainData.Append((ushort)remainingHHPulses10).Append((uint)avgPulseIntervalCurrentMs).Append(pulseRecord);

        var podTime125ms = PodTime.Ticks / 10 / 1000 / 125;
        var halfHour125ms = 30 * 60 * 1000 / 125;
        var spentCurrentHH125ms = podTime125ms % halfHour125ms;
        var toNextHH125ms = halfHour125ms - spentCurrentHH125ms;

        var currentHHPulses = hhPulses[currentHalfHour];
        var remainingHHPulses = currentHHPulses - currentHHPulses * spentCurrentHH125ms / halfHour125ms;

        var scheduleData = ScheduleHelper.GetScheduleDataWithChecksum(
            (byte)currentHalfHour,
            (ushort)toNextHH125ms,
            (ushort)remainingHHPulses,
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
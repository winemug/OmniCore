using OmniCore.Common.Entities;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;

namespace OmniCore.Framework.Omnipod.Requests;

public class StartTempBasalMessage : IMessageData
{
    public int HalfHourCount { get; set; }
    public int PulsesPerHour { get; set; }
    public PulseSchedule Schedule { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts => parts.MainPart.Type == PodMessagePartType.RequestTempBasal &&
                 parts.SubPart?.Type == PodMessagePartType.RequestInsulinSchedule;

    public IMessageData FromParts(IMessageParts parts)
    {
        Schedule = ScheduleHelper.ParsePulseSchedule(parts.MainPart.Data, false, false);
        
        var subData = parts.SubPart!.Data.Sub(4);
        var iss = ScheduleHelper.ParseInsulinScheduleData(subData);
        HalfHourCount = 0;
        foreach (var schedule in iss.Entries.Skip(1)) HalfHourCount += schedule.BlockCount;
        PulsesPerHour = iss.Entries[0].PulsesPerBlock * 2
                        + (iss.Entries[0].AddAlternatingExtraPulse ? 1 : 0);

        return this;
    }

    public IMessageParts ToParts()
    {
        var mainData = new Bytes();
        mainData.Append(0).Append(0);

        var totalPulses10 = PulsesPerHour * HalfHourCount * 10 / 2;
        var hhPulses10 = PulsesPerHour * 10 / 2;

        var avgPulseIntervalMs = 1800000000;
        if (PulsesPerHour > 0)
            avgPulseIntervalMs = 360000000 / PulsesPerHour;

        var pulseRecord = new Bytes();
        if (totalPulses10 == 0)
        {
            for (var i = 0; i < HalfHourCount; i++)
                pulseRecord.Append((ushort)0).Append((uint)avgPulseIntervalMs);
        }
        else
        {
            var pulses10Remaining = totalPulses10;
            while (pulses10Remaining > 0)
            {
                var pulses10Record = pulses10Remaining;
                if (pulses10Remaining > 0xFFFF)
                {
                    if (hhPulses10 > 0xFFFF)
                    {
                        pulses10Record = 0XFFFF;
                    }
                    else
                    {
                        var hhCountFitting = 0xFFFF / hhPulses10;
                        if (hhCountFitting % 2 == 0)
                            hhCountFitting--;
                        pulses10Record = hhCountFitting * hhPulses10;
                    }
                }

                pulseRecord.Append((ushort)pulses10Record).Append((uint)avgPulseIntervalMs);
                pulses10Remaining -= pulses10Record;
            }
        }

        mainData.Append(pulseRecord.Sub(0, 6)).Append(pulseRecord);

        var schedules = new[]
        {
            new InsulinScheduleEntry
            {
                BlockCount = HalfHourCount,
                AddAlternatingExtraPulse = PulsesPerHour % 2 == 1,
                PulsesPerBlock = PulsesPerHour / 2
            }
        };

        var scheduleData = ScheduleHelper.GetScheduleDataWithChecksum(
            (byte)HalfHourCount,
            30 * 60 * 8,
            (ushort)(PulsesPerHour / 2),
            schedules);

        var subData = new Bytes(1).Append(scheduleData);

        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestTempBasal,
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
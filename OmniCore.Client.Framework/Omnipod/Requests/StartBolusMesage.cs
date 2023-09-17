using OmniCore.Common.Entities;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;

namespace OmniCore.Framework.Omnipod.Requests;

public class StartBolusMesage : IMessageData
{
    public int ImmediatePulseCount { get; set; }
    public int ImmediatePulseIntervalMilliseconds { get; set; }

    public bool SpecialBolus { get; set; }
    public int ExtendedPulseCount { get; set; }
    public int ExtendedHalfHourCount { get; set; }
    
    public PulseSchedule Schedule { get; set; }

    public static Predicate<IMessageParts> CanParse =>
        parts => parts.MainPart.Type == PodMessagePartType.RequestBolus &&
                 parts.SubPart?.Type == PodMessagePartType.RequestInsulinSchedule;

    public IMessageData FromParts(IMessageParts parts)
    {
        Schedule = ScheduleHelper.ParsePulseSchedule(parts.MainPart.Data, false, true);
        var mainData = parts.MainPart.Data;
        var totalPulses10 = mainData.Word(1);
        var pulseInterval = mainData.DWord(3);

        ImmediatePulseCount = totalPulses10 / 10;
        ImmediatePulseIntervalMilliseconds = (int)(pulseInterval / 100);

        return this;
    }

    public IMessageParts ToParts()
    {
        var mainData = new Bytes();
        mainData.Append(0);
        var totalPulses10 = (ushort)(ImmediatePulseCount * 10);
        var pulseInterval = (uint)(ImmediatePulseIntervalMilliseconds * 100);
        Bytes subData;


        if (SpecialBolus && ImmediatePulseCount > 2)
        {
            uint waitInterval = 10 * 1000 * 100;
            var aPulse = ImmediatePulseCount / 2;
            var bPulse = ImmediatePulseCount - aPulse - 1;
            var aPulses10 = (ushort)(aPulse * 10);
            var bPulses10 = (ushort)(bPulse * 10);
            mainData.Append(aPulses10).Append(pulseInterval).Append((ushort)10).Append(waitInterval);
            mainData.Append(bPulses10).Append(pulseInterval);
            var schedules = new[]
            {
                new InsulinScheduleEntry
                {
                    BlockCount = 1,
                    AddAlternatingExtraPulse = false,
                    PulsesPerBlock = aPulse
                },
                new InsulinScheduleEntry
                {
                    BlockCount = 1,
                    AddAlternatingExtraPulse = false,
                    PulsesPerBlock = 1
                },
                new InsulinScheduleEntry
                {
                    BlockCount = 1,
                    AddAlternatingExtraPulse = false,
                    PulsesPerBlock = bPulse
                }
            };
            var scheduleData = ScheduleHelper.GetScheduleDataWithChecksum(
                2,
                (ushort)(aPulse * ImmediatePulseIntervalMilliseconds / 125),
                (ushort)aPulse,
                schedules);

            subData = new Bytes(2).Append(scheduleData);
        }
        else
        {
            mainData.Append(totalPulses10).Append(pulseInterval).Append((ushort)0).Append((uint)0);
            var schedules = new[]
            {
                new InsulinScheduleEntry
                {
                    BlockCount = 1,
                    AddAlternatingExtraPulse = false,
                    PulsesPerBlock = ImmediatePulseCount
                }
            };
            var scheduleData = ScheduleHelper.GetScheduleDataWithChecksum(
                1,
                (ushort)(ImmediatePulseCount * ImmediatePulseIntervalMilliseconds / 125),
                (ushort)ImmediatePulseCount,
                schedules);

            subData = new Bytes(2).Append(scheduleData);
        }


        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestBolus,
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
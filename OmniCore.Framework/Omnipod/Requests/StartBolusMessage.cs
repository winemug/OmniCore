using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Messages;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Requests;

public class StartBolusMesage : IMessageData
{
    public static Predicate<IMessageParts> CanParse =>
        (parts) => parts.MainPart.Type == PodMessagePartType.RequestBolus &&
                   parts.SubPart?.Type == PodMessagePartType.RequestInsulinSchedule;

    public int ImmediatePulseCount { get; set; }
    public int ImmediatePulseIntervalMilliseconds { get; set; }
    public int ExtendedPulseCount { get; set; }
    public int ExtendedHalfHourCount { get; set; }

    public IMessageData FromParts(IMessageParts parts)
    {
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
        ushort totalPulses10 = (ushort)(ImmediatePulseCount * 10);
        uint pulseInterval = (uint)(ImmediatePulseIntervalMilliseconds * 100);
        mainData.Append(totalPulses10).Append(pulseInterval).Append((ushort)0).Append((uint)0);

        var schedules = new[]
        {
            new InsulinSchedule
            {
                BlockCount = 1,
                AddAlternatingExtraPulse = false,
                PulsesPerBlock = ImmediatePulseCount
            }
        };
        var scheduleData = ScheduleHelper.GetScheduleDataWithChecksum(
            1,
            (ushort)(ImmediatePulseCount * ImmediatePulseIntervalMilliseconds / 125),
            (ushort)(ImmediatePulseCount),
            schedules);

        var subData = new Bytes(2).Append(scheduleData);

        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestBolus,
                RequiresNonce = false,
                Data = mainData,
            },
            new MessagePart
            {
                Type = PodMessagePartType.RequestInsulinSchedule,
                RequiresNonce = true,
                Data = subData,
            });
    }
}

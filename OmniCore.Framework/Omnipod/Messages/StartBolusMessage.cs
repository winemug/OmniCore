﻿using OmniCore.Common.Pod;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Messages;

public class StartBolusMesage : IMessageData
{
    public static Predicate<IMessageParts> CanParse =>
        (parts) => parts.MainPart.Type == PodMessagePartType.RequestBolus &&
                   parts.SubPart?.Type == PodMessagePartType.RequestInsulinSchedule;

    public int ImmediatePulseCount { get; set; }
    public long ImmediatePulseIntervalMs { get; set; }
    public int ExtendedPulseCount { get; set; }
    public int ExtendedHalfHourCount { get; set; }

    public IMessageData FromParts(IMessageParts parts)
    {
        return this;
    }

    public IMessageParts ToParts()
    {
        var mainData = new Bytes();
        mainData.Append(0);
        ushort totalPulses10 = (ushort)(ImmediatePulseCount * 10);
        uint pulseInterval = (uint)(ImmediatePulseIntervalMs * 100);
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
            (ushort)(ImmediatePulseCount * ImmediatePulseIntervalMs / 125),
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

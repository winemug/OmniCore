using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RequestInsulinSchedulePart : MessagePart
{
    public RequestInsulinSchedulePart(BasalRateEntry[] basalRateEntries)
    {
        var hhPulses = DistributeHourlyRatesToHalfHourPulses(basalRateEntries);
    }

    public RequestInsulinSchedulePart(BasalRateEntry tempBasalEntry)
    {
        var schedules = new[]
        {
            new InsulinSchedule
            {
                BlockCount = tempBasalEntry.HalfHourCount,
                AddAlternatingExtraPulse = tempBasalEntry.PulsesPerHour % 2 == 1,
                PulsesPerBlock = tempBasalEntry.PulsesPerHour / 2
            }
        };
        Data = GetData(ScheduleType.TempBasal,
            (byte)tempBasalEntry.HalfHourCount,
            30 * 60 * 8,
            (ushort)(tempBasalEntry.PulsesPerHour / 2),
            schedules);
    }

    public RequestInsulinSchedulePart(BolusEntry bolusEntry)
    {
        var schedules = new[]
        {
            new InsulinSchedule
            {
                BlockCount = 1,
                AddAlternatingExtraPulse = false,
                PulsesPerBlock = bolusEntry.ImmediatePulseCount
            }
        };
        Data = GetData(ScheduleType.Bolus,
            1,
            (ushort)(bolusEntry.ImmediatePulseCount * bolusEntry.ImmediatePulseInterval125ms),
            (ushort)(bolusEntry.ImmediatePulseCount),
            schedules);
    }

    public override bool RequiresNonce => true;
    public override PodMessageType Type => PodMessageType.RequestInsulinSchedule;

    private Bytes GetData(ScheduleType type,
        byte halfHourCount,
        ushort initialDuration125ms,
        ushort initialPulseCount,
        InsulinSchedule[] schedules
    )
    {
        var elements = new Bytes();
        foreach (var schedule in schedules)
        {
            var scheduleBlocksAdded = 0;
            while (scheduleBlocksAdded < schedule.BlockCount)
            {
                var blockCount = schedule.BlockCount - scheduleBlocksAdded;
                if (blockCount > 16)
                    blockCount = 16;
                var b0 = ((blockCount - 1) & 0x0f) << 4;
                if (schedule.AddAlternatingExtraPulse) b0 |= 0x08;

                b0 |= schedule.PulsesPerBlock >> 8;
                var b1 = schedule.PulsesPerBlock & 0xFF;
                elements.Append((byte)b0).Append((byte)b1);
                scheduleBlocksAdded += blockCount;
            }
        }

        var header = new Bytes(halfHourCount).Append(initialDuration125ms).Append(initialPulseCount);
        var checksum = header[0] + header[1] + header[2] + header[3] + header[4];

        // 'generated' table
        var hh_idx = 0;
        foreach (var schedule in schedules)
            for (var i = 0; i < schedule.BlockCount; i++)
            {
                var pw = schedule.PulsesPerBlock;
                if (schedule.AddAlternatingExtraPulse && hh_idx % 2 == 1)
                    pw += 1;
                checksum += (pw >> 8) & 0xFF;
                checksum += pw & 0xFF;
                hh_idx++;
            }

        return new Bytes((byte)type).Append((ushort)checksum).Append(header).Append(elements);
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
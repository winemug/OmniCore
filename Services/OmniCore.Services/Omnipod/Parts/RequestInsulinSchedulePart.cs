using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public enum ScheduleType
{
    Basal = 0,
    TempBasal = 1,
    Bolus = 2
}

public struct BolusEntry
{
    public int ImmediatePulseCount { get; set; }
    public int ImmediatePulseInterval125ms { get; set; }
    public int ExtendedHalfHourCount { get; set; }
    public int ExtendedTotalPulseCount { get; set; }
}

public struct BasalRateEntry
{ 
    public int HalfHourCount { get; set; }
    public int PulsesPerHour { get; set; }
}

public struct InsulinSchedule
{
    public int BlockCount { get; set; } // 8 bits
    public bool AddAlternatingExtraPulse { get; set; }
    public int PulsesPerBlock { get; set; } // 10bits
}
public class RequestInsulinSchedulePart : MessagePart
{
    public override bool RequiresNonce => true;
    public override PodMessageType Type => PodMessageType.RequestInsulinSchedule;

    public RequestInsulinSchedulePart(BasalRateEntry[] basalRateEntries)
    {
        var hhPulses = DistributeHourlyRatesToHalfHourPulses(basalRateEntries);
    }
    public RequestInsulinSchedulePart(BasalRateEntry tempBasalEntry)
    {
        var schedules = new[]
        {
            new InsulinSchedule()
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
    }
    
    private Bytes GetData(ScheduleType type,
        byte halfHourCount, ushort initialDuration125ms, ushort initialPulseCount,
        InsulinSchedule[] schedules
        )
    {
        var elements = new Bytes();
        foreach (var schedule in schedules)
        {
            var b0 = ((schedule.BlockCount - 1) & 0x0f) << 4;
            if (schedule.AddAlternatingExtraPulse)
            {
                b0 |= 0x08;
            }

            b0 |= schedule.PulsesPerBlock >> 8;
            var b1 = schedule.PulsesPerBlock & 0xFF;
            elements.Append((byte)b0).Append((byte)b1);
        }

        var header = new Bytes(halfHourCount).Append(initialDuration125ms).Append(initialPulseCount);
        int checksum = header[0] + header[1] + header[2] + header[3] + header[4];
        
        // 'generated' table
        int hh_idx = 0;
        foreach (var schedule in schedules)
        {
            for (int i = 0; i < schedule.BlockCount; i++)
            {
                int pw = schedule.PulsesPerBlock;
                if (schedule.AddAlternatingExtraPulse && hh_idx % 2 == 1)
                    pw += 1;
                checksum += (pw >> 8) & 0xFF;
                checksum += pw & 0xFF;
                hh_idx++;
            }
        }
        return new Bytes((byte)type).Append((ushort)checksum).Append(header).Append(elements);
    }
    
    private int[] DistributeHourlyRatesToHalfHourPulses(BasalRateEntry[] entries)
    {
        var hourlyRatePer30Mins = new int[48];
        var halfHourCount = 0;
        foreach (var entry in entries)
        {
            for (int i = 0; i < entry.HalfHourCount; i++)
            {
                hourlyRatePer30Mins[halfHourCount] = entry.PulsesPerHour;
                halfHourCount++;
            }
        }

        var pulsesPerHalfHour = new int[halfHourCount];
        var overflow = false;
        for (int i = 0; i < halfHourCount; i++)
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
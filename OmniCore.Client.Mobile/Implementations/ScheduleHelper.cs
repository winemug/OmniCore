using System.Formats.Tar;
using OmniCore.Common.Entities;

namespace OmniCore.Common.Pod;


// 1A Insulin Schedule
// (long) nonce
// (byte) Table No: 00 Basal, 01 Temp Basal, 02 Bolus
// (dword) checksum

// (byte) Basal: Current HalfHour of Day
//        Temp: Count of "total, i.e. resulting" Halfhours (excluding initial)
//        Bolus: 1+Extended half hours
// (word) Duration of initial schedule, in Seconds * 8
// (word) Total pulses in initial schedule, Pulses

// Schedule entry (repeated)
// (word)
// 1 bit: Flag for extra pulse "each alternate half hour" // whether hourcount or what really
// 3 bits: Half Hour Repeats (min 1, max 8) (3bits+1)
// 2 bits: Unused?
// 10 bits: Total Pulses in block

// Bolus:??? Repeated entry for the initial duration pulses, as if it's an half hour. 


// Pulse Schedule 13 16 17

// BASAL
// (byte) beep
// (byte) (basal) hh index for which initial, (temp) 00, (bolus) **** NO SUCH FIELD ***
//        (temp) 0

// (word) Remaining pulses*10 (basal, temp), immediate total pulses*10 (bolus) 0 if no immediate bolus
// (dword) Microseconds until next pulse/10 (max 0x6b49d200), (bolus) 0x30d40 (2 secs) even if no immediate bolus

// (word) Total pulses*10
// (dword) Microseconds interval between pulses (max 0x6b49d200)

// TEMP


public static class ScheduleHelper
{
    public static InsulinSchedule ParseInsulinScheduleData(Bytes data)
    {
        var iss = new InsulinSchedule();
        
        var tableNumber = data[0];
        var halfHourIndicator = data[3];
        var initialDuration125Ms = data.Word(4);
        var initialPulseCount = data.Word(6);

        iss.InitialDurationMilliseconds = (ulong)(initialDuration125Ms) * 125;
        iss.InitialDurationPulseCount = initialPulseCount;
        iss.HalfHourIndicator = halfHourIndicator;
        var idx = 8;
        var schedules = new List<InsulinScheduleEntry>();
        while (idx < data.Length)
        {
            var b0 = data[idx++];
            var b1 = data[idx++];

            var blockCount = (b0 >> 4) + 1;
            var alternatingExtraPulse = (b0 & 0x08) > 0;
            var pulsesPerBlock = (b0 & 0x07) << 8;
            pulsesPerBlock |= b1;
            schedules.Add(new InsulinScheduleEntry
            {
                BlockCount = blockCount,
                PulsesPerBlock = pulsesPerBlock,
                AddAlternatingExtraPulse = alternatingExtraPulse
            });
        }

        iss.Entries = schedules.ToArray();
        return iss;
    }

    public static PulseSchedule ParsePulseSchedule(Bytes mainData, bool rolling, bool bolus)
    {
        var idx = 2;
        if (bolus)
            idx = 1;            
        var schedules = new List<PulseScheduleEntry>();
        while (idx < mainData.Length)
        {
            schedules.Add(new PulseScheduleEntry
            {
                CountDecipulses = mainData.Word(idx),
                IntervalMicroseconds = mainData.DWord(idx + 2)
            });
            idx += 6;
        }

        if (bolus)
            return new PulseSchedule(
                schedules[0].CountDecipulses,
                schedules[0].IntervalMicroseconds,
                0,
                schedules.ToArray(),
                rolling);
        return new PulseSchedule(
            schedules[0].CountDecipulses,
            schedules[0].IntervalMicroseconds,
            mainData[1],
            schedules.Skip(1).ToArray(),
            rolling);
    }

    public static Bytes GetScheduleDataWithChecksum(
        byte halfHourCount,
        ushort initialDuration125Ms,
        ushort initialPulseCount,
        InsulinScheduleEntry[] schedules)
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

        var header = new Bytes(halfHourCount).Append(initialDuration125Ms).Append(initialPulseCount);
        var checksum = header[0] + header[1] + header[2] + header[3] + header[4];

        var hhIdx = 0;
        foreach (var schedule in schedules)
            for (var i = 0; i < schedule.BlockCount; i++)
            {
                var pw = schedule.PulsesPerBlock;
                if (schedule.AddAlternatingExtraPulse && hhIdx % 2 == 1)
                    pw += 1;
                checksum += (pw >> 8) & 0xFF;
                checksum += pw & 0xFF;
                hhIdx++;
            }

        return new Bytes((ushort)checksum).Append(header).Append(elements);
    }

    public static InsulinScheduleEntry[] GetConsecutiveSchedules(int[] hhPulses)
    {
        List<InsulinScheduleEntry> schedules = new();

        var schedule = new InsulinScheduleEntry
        {
            BlockCount = 0,
            AddAlternatingExtraPulse = false,
            PulsesPerBlock = hhPulses[0]
        };

        var currentBlock = 0;
        foreach (var hhPulse in hhPulses)
        {
            var oldBlockCount = schedule.BlockCount;
            var oldPulsesPerBlock = schedule.PulsesPerBlock;
            var oldAlternatingMode = schedule.AddAlternatingExtraPulse;
            var addNewSchedule = false;
            if (schedule.BlockCount < 2)
            {
                if (schedule.PulsesPerBlock == hhPulse)
                    schedule = new InsulinScheduleEntry
                    {
                        BlockCount = oldBlockCount + 1,
                        AddAlternatingExtraPulse = false,
                        PulsesPerBlock = oldPulsesPerBlock
                    };
                else if (schedule.PulsesPerBlock == hhPulse - currentBlock % 2)
                    schedule = new InsulinScheduleEntry
                    {
                        BlockCount = oldBlockCount + 1,
                        AddAlternatingExtraPulse = true,
                        PulsesPerBlock = oldPulsesPerBlock
                    };
                else
                    addNewSchedule = true;
            }
            else
            {
                var hhPulsesCompare = oldAlternatingMode ? hhPulse - currentBlock % 2 : hhPulse;
                if (schedule.PulsesPerBlock == hhPulsesCompare)
                    schedule = new InsulinScheduleEntry
                    {
                        BlockCount = oldBlockCount + 1,
                        AddAlternatingExtraPulse = oldAlternatingMode,
                        PulsesPerBlock = oldPulsesPerBlock
                    };
                else
                    addNewSchedule = true;
            }

            if (addNewSchedule)
            {
                schedules.Add(schedule);
                schedule = new InsulinScheduleEntry
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
}
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Framework.Omnipod.Messages;

public static class ScheduleHelper
{
    public static Bytes GetScheduleDataWithChecksum(byte halfHourCount,
        ushort initialDuration125ms,
        ushort initialPulseCount,
        InsulinSchedule[] schedules)
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

        return new Bytes((ushort)checksum).Append(header).Append(elements);
    }

    public static InsulinSchedule[] GetConsecutiveSchedules(int[] hhPulses)
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
}

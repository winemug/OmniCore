using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Pod;

public class ScheduleRequest
{
    private int[] _decipulses = new int[0];
    private int[] _decipulseIntervalsMicroseconds = new int[0];
    private int _initialDecipulses = 0;
    private int _initialDecipulseIntervalMicroseconds = 0;

    private List<int> _halfHourPulses = new List<int>();

    private List<InsulinSchedule> _schedules = new List<InsulinSchedule>();


    public ScheduleRequest ForBolus(int pulseCount, int intervalMilliseconds)
    {
        _initialDecipulses = pulseCount * 10;
        _initialDecipulseIntervalMicroseconds = intervalMilliseconds * 1000 / 10;
        return this;
    }

    public ScheduleRequest ForExtendedBolus(int immediatePulseCount, int immediatePulseIntervalMilliseconds,
        int extendedPulseCount, int extensionHalfHourCount)
    {
        _initialDecipulses = immediatePulseCount * 10;
        _initialDecipulseIntervalMicroseconds = immediatePulseIntervalMilliseconds * 1000 / 10;

        var decipulses = new List<int>();
        var decipulseIntervals = new List<int>();

        _decipulses = new int[extensionHalfHourCount];
        return this;
    }

    public ScheduleRequest ForFixedTempBasal(int pulsesPerHour, int halfHourCount)
    {
        return this;
    }

    public ScheduleRequest ForDailySchedule(TimeOnly podTime, int[] pulsesPerHourFor48HalfHours)
    {
        return this;
    }

    public void Split(TimeSpan split,
        decimal unitsPerMicroliter, decimal microLitersPerPulse,
        InsulinPreciseSchedule preSplitSchedule,
        InsulinPreciseSchedule[] schedules)
    {
        var splitTicks = split.Ticks;

        var microLiters = preSplitSchedule.Units / unitsPerMicroliter;
        var pulses10 = (microLiters * 10 / microLitersPerPulse);
        var pulses10int = (long)pulses10;
        var intervalTicks = preSplitSchedule.Duration.Ticks / pulses10int;

        var pulsesCarriedOver = pulses10 - pulses10int;

        Debug.WriteLine($"Interval #0: Deliver {pulses10int} pulses at {intervalTicks}. CarryOver: {pulsesCarriedOver}");

        foreach (var schedule in schedules)
        {
            microLiters = schedule.Units / unitsPerMicroliter;
            pulses10 = (microLiters * 10 / microLitersPerPulse) + pulsesCarriedOver;
            pulses10int = (long)pulses10;

            intervalTicks = schedule.Duration.Ticks / pulses10int;
            var pulseTicks = new long[pulses10int];
            for (int i = 0; i < pulses10int; i++)
                pulseTicks[i] = (i + 1) * intervalTicks;

            long t0 = 0;
            var p = 0;
            while (t0 < schedule.Duration.Ticks)
            {
                var t1 = t0 + splitTicks;
                var count = pulseTicks.Where(t => t > t0 && t <= t1).Count();
                pulses10 -= count;
                Debug.WriteLine($"Interval #{p+1}: Deliver {count} pulses at {intervalTicks}. CarryOver: {pulses10}");
                t0 = t1;
                p++;
            }
            pulsesCarriedOver += pulses10;
        }
    }
    private InsulinSchedule GetSchedule(int decipulses, int blockCount)
    {
        return new InsulinSchedule
        {
            BlockCount = blockCount,
            PulsesPerBlock = decipulses / 10,
            AddAlternatingExtraPulse = (decipulses % 10) > 5
        };
    }
}

public struct InsulinPreciseSchedule
{
    public decimal Units { get; set; }
    public TimeSpan Duration { get; set; }
}

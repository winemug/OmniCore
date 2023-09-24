using System.Diagnostics;

namespace OmniCore.Common.Pod;

public class PulseSchedule
{
    private int _totalDecipulses = 0;
    private ushort _remainingDecipulses;
    private ulong _microsecondsToNextDecipulse;
    private int _entryIndex;
    private bool _rolling;
    private bool _repeatLast;
    private readonly PulseScheduleEntry[] _entries;
    private const ulong HalfHourMicroseconds = 30 * 60 * 1000 * 1000;

    public int CurrentHalfHours { get; private set; }
    public ulong CurrentMicroseconds { get; private set; }
    
    public DateTimeOffset ScheduleStart { get; set; } = DateTimeOffset.MinValue;
    
    public PulseSchedule(
        ushort remainingDecipulses,
        ulong microsecondsToNextDecipulse,
        int entryIndex,
        PulseScheduleEntry[] entries,
        bool rolling = false)
    {
        _remainingDecipulses = remainingDecipulses;
        _microsecondsToNextDecipulse = microsecondsToNextDecipulse;
        _entryIndex = entryIndex;
        _entries = entries;
        _rolling = rolling;
    }
   
    public DateTimeOffset? GetNext(bool asDecipulses = false)
    {
        if (_repeatLast)
        {
            _repeatLast = false;
            if (_totalDecipulses % 10 == 0 || asDecipulses)
                return ScheduleStart + TimeSpan.FromMinutes(30*CurrentHalfHours) + TimeSpan.FromMicroseconds(CurrentMicroseconds);
        }
        
        if (_entryIndex >= _entries.Length)
            return null;
        var entry = _entries[_entryIndex];
        int runOut = 320;
        while (runOut-- > 0)
        {
            if (_remainingDecipulses == 0)
            {
                CurrentMicroseconds += _microsecondsToNextDecipulse;
                if (CurrentMicroseconds > HalfHourMicroseconds)
                {
                    CurrentMicroseconds -= HalfHourMicroseconds;
                    CurrentHalfHours += 1;
                }

                _entryIndex++;
                if (_entryIndex >= _entries.Length)
                {
                    if (!_rolling)
                        return null;
                    _entryIndex = 0;
                }

                entry = _entries[_entryIndex];
                _remainingDecipulses = entry.CountDecipulses;
                _microsecondsToNextDecipulse = entry.IntervalMicroseconds;
                continue;
            }

            _remainingDecipulses--;

            CurrentMicroseconds += _microsecondsToNextDecipulse;
            if (CurrentMicroseconds > HalfHourMicroseconds)
            {
                CurrentMicroseconds -= HalfHourMicroseconds;
                CurrentHalfHours += 1;
            }
            _totalDecipulses++;
            if (_totalDecipulses % 10 == 0 || asDecipulses)
                return ScheduleStart + TimeSpan.FromMinutes(30*CurrentHalfHours) + TimeSpan.FromMicroseconds(CurrentMicroseconds);
        }
        return null;
    }

    public void SeekTo(DateTimeOffset messageTimeEarliest)
    {
        DateTimeOffset dtt = DateTimeOffset.MinValue;
        while (dtt < messageTimeEarliest)
        {
            var dx = GetNext(true);
            if (dx == null)
                return;
            dtt = dx.Value;
        }
        _repeatLast = true;
    }
}
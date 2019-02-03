using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enums
{
    [Flags]
    public enum Alarm
    {
        None = 0,
        AutoOff = 1,
        Unknown = 2,
        EndOfService = 4,
        Expired = 8,
        LowReservoir = 16,
        SuspendInProgress = 32,
        SuspendEnded = 64,
        TimerLimit = 128
    }
}

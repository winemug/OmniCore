using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enumerations
{
    public enum PodProgress
    {
        InitialState = 0,
        TankPowerActivated = 1,
        TankFillCompleted = 2,
        PairingSuccess = 3,
        Purging = 4,
        ReadyForInjection = 5,
        BasalScheduleSet = 6,
        Priming = 7,
        Running = 8,
        RunningLow = 9,
        Unknown10 = 10,
        Unknown11 = 11,
        Unknown12 = 12,
        ErrorShuttingDown = 13,
        AlertExpiredShuttingDown = 14,
        Inactive = 15
    }
}

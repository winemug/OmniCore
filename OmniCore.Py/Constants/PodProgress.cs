namespace OmniCore.Py
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
        Inserting = 7,
        Running = 8,
        RunningLow = 9,
        ErrorShuttingDown = 13,
        AlertExpiredShuttingDown = 14,
        Inactive = 15
    }
}

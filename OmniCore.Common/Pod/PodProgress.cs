namespace OmniCore.Services.Interfaces.Pod;

public enum PodProgress
{
    NotInitialized = 0,
    TankPowerActivated = 1,
    TankFillCompleted = 2,
    Paired = 3,
    Priming = 4,
    Primed = 5,
    BasalSet = 6,
    Inserting = 7,
    Running = 8,
    RunningLow = 9,
    Unknown10 = 10,
    Unknown11 = 11,
    Unknown12 = 12,
    Faulted = 13,
    ActivationExpired = 14,
    Inactive = 15
}
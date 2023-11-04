namespace OmniCore.Shared.Enums;

public enum PodState
{
    JustCreatedLayingAround,
    PrimedStillLayingAround,
    AttachedOnBodyToBeStarted,
    AttachedAndRunning,
    RemovedFallenOffLostAndRunning,
    RemovedAndDeactivated,
    AttachedAndDeactivated,
    RemovedAndFailed,
    AttachedAndFailed,
    FailedAndThrownAwayNoWayToDeactivate
}
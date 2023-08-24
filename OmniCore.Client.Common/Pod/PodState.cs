namespace OmniCore.Common.Pod;

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
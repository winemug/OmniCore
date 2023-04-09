namespace OmniCore.Services.Interfaces.Pod;

public enum PodRequestStatus
{
    Executed,
    Inconclusive,
    NotSubmitted,
    RejectedByApp,
    RejectedByPod,
}
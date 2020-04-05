namespace OmniCore.Model.Enumerations
{
    public enum ProgressPhase
    {
        Waiting,
        Initializing,
        Running,
        Finalizing,
        CompletedSuccessfully,
        CompletedError,
        Canceled
    }
}
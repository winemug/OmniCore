namespace OmniCore.Model.Enumerations
{
    public enum TaskStatus
    {
        Scheduled,
        Started,
        CannotCancel,
        Finished,
        Canceled,
        Failed,
        MadeRedundant,
        Rescheduled,
        ResultLinked
    }
}
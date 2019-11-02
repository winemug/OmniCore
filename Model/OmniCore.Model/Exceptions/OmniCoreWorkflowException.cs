namespace OmniCore.Model.Exceptions
{
    using OmniCore.Repository.Enums;
    using System;

    public class OmniCoreWorkflowException : OmniCoreException
    {
        public OmniCoreWorkflowException(FailureType failureType, string message = "Unknown", Exception innerException = null)
            : base(failureType, message, innerException)
        {
        }
    }
}

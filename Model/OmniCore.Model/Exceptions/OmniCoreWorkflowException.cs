using OmniCore.Model.Enumerations;
using System;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreWorkflowException : OmniCoreException
    {
        public OmniCoreWorkflowException(FailureType failureType, string message = "Unknown", Exception innerException = null)
            : base(failureType, message, innerException)
        {
        }
    }
}

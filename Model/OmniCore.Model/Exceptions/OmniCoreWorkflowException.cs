using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreWorkflowException : OmniCoreException
    {
        public OmniCoreWorkflowException(FailureType failureType, string message = null, Exception inner = null) : base(
            failureType, message, inner)
        {
        }
    }
}
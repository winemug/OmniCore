using OmniCore.Model.Enumerations;
using System;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreTimeoutException : OmniCoreException
    {
        public OmniCoreTimeoutException(FailureType failureType, string message = "Timeout error") : base(failureType, message) { }
    }
}

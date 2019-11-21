using OmniCore.Model.Enumerations;
using System;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreRadioException : OmniCoreException
    {
        public OmniCoreRadioException(FailureType failureType, string message = "Unknown", Exception inner = null) : base(failureType, message, inner)
        {
        }
    }
}

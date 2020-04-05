using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreRadioException : OmniCoreException
    {
        public OmniCoreRadioException(FailureType failureType, string message = null, Exception inner = null) : base(
            failureType, message, inner)
        {
        }
    }
}
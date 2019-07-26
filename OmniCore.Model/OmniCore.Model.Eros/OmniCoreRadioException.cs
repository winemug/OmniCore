using System;
using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;

namespace OmniCore.Model.Eros
{
    public class OmniCoreRadioException : OmniCoreException
    {
        public OmniCoreRadioException(FailureType failureType, string message = "Unknown radio error", Exception innerException = null) : base(failureType, message, innerException)
        {
        }
    }
}

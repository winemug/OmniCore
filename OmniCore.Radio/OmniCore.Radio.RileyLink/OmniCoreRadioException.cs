using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using System;

namespace OmniCore.Radio.RileyLink
{
    public class OmniCoreRadioException : OmniCoreException
    {
        public OmniCoreRadioException(FailureType failureType, string message = "Unknown radio error", Exception innerException = null) : base(failureType, message, innerException)
        {
        }
    }
}

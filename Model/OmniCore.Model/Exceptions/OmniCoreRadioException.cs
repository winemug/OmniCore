using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreRadioException : OmniCoreException
    {
        public OmniCoreRadioException(FailureType failureType, string message = "Unknown", Exception inner = null) : base(failureType, message, inner)
        {
        }
    }
}

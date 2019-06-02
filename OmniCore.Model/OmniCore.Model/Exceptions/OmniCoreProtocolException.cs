using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreProtocolException : OmniCoreException
    {
        public OmniCoreProtocolException(FailureType failureType, string message = "Protocol error") : base(failureType, message)
        {
        }
    }
}

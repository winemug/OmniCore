using OmniCore.Model.Enumerations;
using System;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreProtocolException : OmniCoreException
    {
        public OmniCoreProtocolException(FailureType failureType, string message = "Protocol error") : base(failureType, message)
        {
        }
    }
}

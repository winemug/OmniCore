using OmniCore.Model.Enumerations;
using System;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreException : Exception
    {
        public FailureType FailureType { get;  }

        public OmniCoreException(FailureType failureType, string message = "Unknown", Exception inner = null) : base(message, inner)
        {
            FailureType = failureType;
        }
    }
}

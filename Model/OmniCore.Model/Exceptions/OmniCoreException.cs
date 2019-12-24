using OmniCore.Model.Enumerations;
using System;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreException : Exception
    {
        public FailureType FailureType { get;  }

        protected OmniCoreException(FailureType failureType, string message = null, Exception inner = null) : base(message, inner)
        {
            FailureType = failureType;
        }
    }
}

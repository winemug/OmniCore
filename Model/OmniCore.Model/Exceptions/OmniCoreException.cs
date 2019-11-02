namespace OmniCore.Model.Exceptions
{
    using OmniCore.Repository.Enums;
    using System;

    public class OmniCoreException : Exception
    {
        public FailureType FailureType { get;  }

        public OmniCoreException(FailureType failureType, string message = "Unknown", Exception inner = null) : base(message, inner)
        {
            FailureType = failureType;
        }
    }
}

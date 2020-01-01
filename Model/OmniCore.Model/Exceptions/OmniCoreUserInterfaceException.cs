using OmniCore.Model.Enumerations;
using System;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreUserInterfaceException : OmniCoreException
    {
        public OmniCoreUserInterfaceException(FailureType failureType, string message = null, Exception inner = null) : base(failureType, message, inner)
        {
        }
    }
}

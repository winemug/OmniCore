using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreRepositoryException : OmniCoreException
    {
        public OmniCoreRepositoryException(FailureType failureType, string message = null, Exception inner = null) :
            base(failureType, message, inner)
        {
        }
    }
}
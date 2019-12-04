using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Exceptions
{
    public class OmniCoreRepositoryException : OmniCoreException
    {
        public OmniCoreRepositoryException(FailureType failureType, string message = "Unknown", Exception inner = null) : base(failureType, message, inner)
        {
        }
    }
}

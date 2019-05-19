using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Exceptions
{
    public class StatusUpdateRequiredException : OmniCoreException
    {
        public StatusUpdateRequiredException(Exception innerException) : base("Recoverable protocol error", innerException)
        {
        }
    }
}

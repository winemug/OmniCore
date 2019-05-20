using OmniCore.Model.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class StatusUpdateRequiredException : OmniCoreException
    {
        public StatusUpdateRequiredException(Exception innerException) : base("Recoverable protocol error", innerException)
        {
        }
    }
}

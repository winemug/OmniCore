using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Py.Exceptions
{
    public class StatusUpdateRequiredException : OmnipyException
    {
        public StatusUpdateRequiredException(Exception innerException) : base("Recoverable protocol error", innerException)
        {
        }
    }
}

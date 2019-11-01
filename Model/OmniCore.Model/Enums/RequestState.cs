using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enums
{
    public enum RequestState
    {
        Queued,
        Scheduled,

        Initializing,
        Executing,
        TryCancel,

        Expired,
        Executed,
        Canceled,
        Failed,
        
        Aborted,
        AbortedWhileExecuting
    }
}

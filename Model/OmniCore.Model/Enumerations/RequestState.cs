using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enumerations
{
    public enum RequestState
    {
        Queued,
        Scheduled,

        WaitingForRadioConnection,
        WaitingToRetryPodConnection,
        Initializing,
        Executing,
        TryingToCancel,

        Expired,
        Executed,
        Canceled,
        Failed,
        
        Aborted,
        AbortedWhileExecuting
    }
}

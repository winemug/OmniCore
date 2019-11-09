using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Enums
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

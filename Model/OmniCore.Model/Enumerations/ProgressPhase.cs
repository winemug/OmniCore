using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enumerations
{
    public enum ProgressPhase
    {
        Waiting,
        Initializing,
        Running,
        Finalizing,
        CompletedSuccessfully,
        CompletedError,
        Canceled
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enumerations
{
    public enum PodState
    {
        Unknown,
        Pairing,
        PendingStart,
        Starting,
        Started,
        Stopped,
        Faulted
    }
}

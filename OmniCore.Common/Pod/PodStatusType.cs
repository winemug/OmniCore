using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Pod;

public enum PodStatusType
{
    Compact = 0,
    Alerts = 1,
    Extended = 2,
    PulseLogRecent = 3,
    Activation = 5,
    PulseLogLast = 80,
    PulseLogPrevious = 81
}
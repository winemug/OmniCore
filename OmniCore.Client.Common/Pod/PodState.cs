using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Pod;

public enum PodState
{
    JustCreatedLayingAround,
    PrimedStillLayingAround,
    AttachedOnBodyToBeStarted,
    AttachedAndRunning,
    RemovedFallenOffLostAndRunning,
    RemovedAndDeactivated,
    AttachedAndDeactivated,
    RemovedAndFailed,
    AttachedAndFailed,
    FailedAndThrownAwayNoWayToDeactivate,

}

using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enums
{
    public enum FailureType
    {
        None,
        RadioNotReachable,
        RadioDisconnectPrematurely,
        RadioRecvTimeout,
        CommunicationInterrupted,
        PodStateInvalidForCommand,
        PodResponseUnexpected,
        PodFaulted,
        Canceled,
        PodResponseUnrecognized,
        Unknown,
        InvalidParameter,
        RadioUnknownError,
        RadioStateError,
        InvalidDataReceived,
        WorkflowError,
        OperationInProgress,
        WakeLockNotAcquired
    }
}

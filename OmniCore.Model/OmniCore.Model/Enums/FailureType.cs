﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enums
{
    public enum FailureType
    {
        RadioNotReachable,
        RadioDisconnectPrematurely,
        RadioRecvTimeout,
        CommunicationInterrupted,
        PodStateInvalidForCommand,
        PodResponseUnexpected,
        PodFaulted,
        PodResponseUnrecognized,
        InvalidParameter,
        RadioUnknownError,
        RadioStateError,
        InvalidDataReceived,
        WorkflowError,
        OperationInProgress,
        WakeLockNotAcquired,
        Unknown,
    }
}

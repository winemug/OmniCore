using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enumerations
{
    public enum FailureType
    {
        AdapterGeneralError,
        AdapterNotEnabled,

        PeripheralGeneralError,
        PeripheralOffline,

        PlatformGeneralError,

        RepositoryGeneralError,
        RepositoryStorageError,

        RadioGeneralError,
        RadioResponseTimeout,
        RadioConnectTimeout,
        RadioDisconnectedPrematurely,
        RadioErrorResponse,
        RadioInvalidResponse,

        WorkflowGeneralError,
        WorkflowRepositoryNotInitialized,
        WorkflowPodStateInvalidForCommand,
        WorkflowPodResponseUnexpected,
        WorkflowPodFaulted,
        WorkflowPodResponseUnrecognized,
        WorkflowInvalidParameter,

        ServiceStartupFailure,
        UserInterfaceInitialization,
        ServiceStopFailure,
        ServiceConnectionFailed,
        Internal,
        ConnectionFailed,
    }
}

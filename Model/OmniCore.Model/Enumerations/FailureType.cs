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
        RadioResponseInterrupted,
        RadioResponseNoData,
        RadioResponseParameterError,
        RadioResponseUnknownCommand
    }
}
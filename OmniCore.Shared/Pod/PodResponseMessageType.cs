namespace OmniCore.Common.Pod;

public enum PodResponseMessageType
{
    Unknown,
    Status,
    VersionShort,
    VersionLong,
    Error,
    ActivationInfo,
    AlertInfo,
    ExtendedInfo,
    PulseLogLast,
    PulseLogPrevious,
    PulseLogRecent
}
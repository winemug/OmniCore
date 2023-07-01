using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Pod;

public enum PodRequestMessageType
{
    Unknown,
    SetRadioAddress,
    SetParameters,
    SetDeliveryFlags,
    UpdateStatus,
    SetAlerts,
    AcknowledgeAlerts,
    SetBasalSchedule,
    StartTempBasal,
    StartBolus,
    CancelDelivery,
    Deactivate,
}


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

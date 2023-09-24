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
    Deactivate
}
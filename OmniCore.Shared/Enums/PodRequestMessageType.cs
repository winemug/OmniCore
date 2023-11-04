namespace OmniCore.Shared.Enums;

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
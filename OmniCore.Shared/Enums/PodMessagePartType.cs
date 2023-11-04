namespace OmniCore.Shared.Enums;

public enum PodMessagePartType
{
    RequestSetupPod = 0x03,
    RequestAssignAddress = 0x07,
    RequestSetDeliveryFlags = 0x08,
    RequestStatus = 0x0e,
    RequestAcknowledgeAlerts = 0x11,
    RequestBasal = 0x13,
    RequestTempBasal = 0x16,
    RequestBolus = 0x17,
    RequestConfigureAlerts = 0x19,
    RequestInsulinSchedule = 0x1a,
    RequestDeactivatePod = 0x1c,
    RequestCancelDelivery = 0x1f,
    RequestBeepConfig = 0x1e,

    ResponseVersionInfo = 0x01,
    ResponseInfo = 0x02,
    ResponseError = 0x06,
    ResponseStatus = 0x1d
}
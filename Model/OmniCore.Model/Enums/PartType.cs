using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Enums
{
    public enum PartType
    {
        ResponseVersionInfo = 0x01,
        ResponseDetailInfoResponse = 0x02,
        RequestSetupPod = 0x03,
        ResponseResyncResponse = 0x06,
        RequestAssignAddress = 0x07,
        RequestSetDeliveryFlags = 0x08,
        RequestStatus = 0x0e,
        RequestAcknowledgeAlerts = 0x11,
        RequestBasalSchedule = 0x13,
        RequestTempBasalSchedule = 0x16,
        RequestBolusSchedule = 0x17,
        RequestConfigureAlerts = 0x19,
        RequestInsulinSchedule = 0x1a,
        RequestDeactivatePod = 0x1c,
        ResponseStatus = 0x1d,
        RequestCancelDelivery = 0x1f
    }
}

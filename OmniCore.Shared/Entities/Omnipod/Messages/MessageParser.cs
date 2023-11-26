using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Shared.Entities.Omnipod.Messages;

public static class MessageParser
{
    public static IMessagePart ParsePart(Span<byte> span)
    {
        switch ((PodMessagePartType)span[0])
        {
            case PodMessagePartType.RequestSetupPod:
                return RequestSetupPod.ToInstance(span[2..]);
            case PodMessagePartType.RequestAssignAddress:
                return RequestAssignAddress.ToInstance(span[2..]);
            case PodMessagePartType.RequestSetDeliveryFlags:
                return RequestSetDeliveryFlags.ToInstance(span[6..]);
            case PodMessagePartType.RequestStatus:
                return RequestStatus.ToInstance(span[2..]);
            case PodMessagePartType.RequestAcknowledgeAlerts:
                return RequestAcknowledgeAlerts.ToInstance(span[6..]);
            case PodMessagePartType.RequestBasal:
                return RequestBasalIntervalSchedule.ToInstance(span[2..]);
            case PodMessagePartType.RequestTempBasal:
                return RequestTempBasalIntervalSchedule.ToInstance(span[2..]);
            case PodMessagePartType.RequestBolus:
                return RequestBolusIntervalSchedule.ToInstance(span[2..]);
            case PodMessagePartType.RequestConfigureAlerts:
                return RequestConfigureAlerts.ToInstance(span[6..]);
            case PodMessagePartType.RequestInsulinSchedule:
                return RequestBlockSchedule.ToInstance(span[6..]);
            case PodMessagePartType.RequestDeactivatePod:
                return RequestDeactivatePod.ToInstance(span[6..]);
            case PodMessagePartType.RequestCancelDelivery:
                return RequestCancelDelivery.ToInstance(span[6..]);
            case PodMessagePartType.RequestBeepConfig:
                return RequestBeepConfig.ToInstance(span[2..]);
            case PodMessagePartType.ResponseStatus:
                return ResponseStatus.ToInstance(span[1..]);
            case PodMessagePartType.ResponseError:
                return ResponseError.ToInstance(span[2..]);
            case PodMessagePartType.ResponseVersionInfo:
                if (span.Length == 23)
                    return ResponseVersionInfo.ToInstance(span[2..]);
                if (span.Length == 29)
                    return ResponseVersionInfoExtended.ToInstance(span[2..]);
                throw new ApplicationException("Failed to parse message part");
            case PodMessagePartType.ResponseInfo:
                return ResponseInfo.ToInstance(span[2..]);
            default:
                throw new ApplicationException("Unrecognized message part type");
        }
    }
}

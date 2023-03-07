using System;
using System.Text.Json;
using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class MessagePart : IMessagePart
{
    public virtual bool RequiresNonce { get; }
    public virtual PodMessageType Type { get; set; }
    public uint Nonce { get; set; }
    public Bytes Data { get; set; }
    public static MessagePart Parse(PodMessageType type, Bytes data)
    {
        switch (type)
        {
            case PodMessageType.ResponseStatus:
                return new ResponseStatusPart(data);
            case PodMessageType.ResponseError:
                return new ResponseErrorPart(data);
            case PodMessageType.ResponseInfo:
                var riType = (RequestStatusType)data[0];
                switch (riType)
                {
                    case RequestStatusType.Alerts:
                        return new ResponseInfoAlertsPart(data);
                    case RequestStatusType.Extended:
                        return new ResponseInfoExtendedPart(data);
                    case RequestStatusType.PulseLogRecent:
                        return new ResponseInfoPulseLogRecentPart(data);
                    case RequestStatusType.Activation:
                        return new ResponseInfoActivationPart(data);
                    case RequestStatusType.PulseLogLast:
                        return new ResponseInfoPulseLogLastPart(data);
                    case RequestStatusType.PulseLogPrevious:
                        return new ResponseInfoPulseLogPreviousPart(data);
                }
                break;
            case PodMessageType.ResponseVersionInfo:
                return new ResponseVersionPart(data);
        }
        return new MessagePart() {Type = type, Data = data};
    }
}
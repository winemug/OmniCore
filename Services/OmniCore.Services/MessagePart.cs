using System;
using System.Text.Json;
using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class MessagePart
{
    public virtual bool RequiresNonce { get; }
    public virtual RadioMessageType Type { get; set; }
    public uint Nonce { get; set; }
    public Bytes Data { get; set; }
    public static MessagePart Parse(RadioMessageType type, Bytes data)
    {
        switch (type)
        {
            case RadioMessageType.ResponseStatus:
                return new ResponseStatusPart(data);
                break;
            case RadioMessageType.ResponseError:
                return new ResponseErrorPart(data);
                break;
            case RadioMessageType.ResponseInfo:
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
            case RadioMessageType.ResponseVersionInfo:
                return new ResponseVersionPart(data);
        }
        return new MessagePart() {Type = type, Data = data};
    }
}
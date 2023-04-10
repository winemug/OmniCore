using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class MessagePart : IMessagePart
{
    public bool RequiresNonce { get; set;  }
    public PodMessagePartType Type { get; set; }
    public uint Nonce { get; set; }
    public Bytes Data { get; set; }

    public static MessagePart Parse(PodMessagePartType type, Bytes data)
    {
        switch (type)
        {
            case PodMessagePartType.ResponseStatus:
                return new ResponseStatusPart(data);
            case PodMessagePartType.ResponseError:
                return new ResponseErrorPart(data);
            case PodMessagePartType.ResponseInfo:
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
            case PodMessagePartType.ResponseVersionInfo:
                return new ResponseVersionPart(data);
        }

        return new MessagePart { Type = type, Data = data };
    }

    public override string ToString()
    {
        return $"Type: {Type:X} Data: {Data}";
    }
}
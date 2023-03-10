using System.Collections.Generic;
using System.Text;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class PodMessage : IPodMessage
{
    public uint Address { get; set; }
    public int Sequence { get; set; }
    public bool WithCriticalFollowup { get; set; }
    public List<IMessagePart> Parts { get; set; }

    public uint? AckAddressOverride { get; set; }
    public Bytes Body { get; set; }

    public Bytes GetBody()
    {
        var bodyLength = 0;
        foreach (var part in Parts)
        {
            if (part.RequiresNonce)
                bodyLength += 4;
            bodyLength += part.Data.Length + 2;
        }

        var messageBody = new Bytes(Address);
        byte b0 = 0x00;
        if (WithCriticalFollowup)
            b0 = 0x80;
        b0 |= (byte)(Sequence << 2);
        b0 |= (byte)((bodyLength >> 8) & 0x03);
        var b1 = (byte)(bodyLength & 0xff);
        messageBody.Append(new[] { b0, b1 });
        foreach (var part in Parts)
        {
            messageBody.Append((byte)part.Type);
            if (part.RequiresNonce)
            {
                messageBody.Append((byte)(part.Data.Length + 4));
                messageBody.Append(part.Nonce);
            }
            else
            {
                messageBody.Append((byte)part.Data.Length);
            }

            messageBody.Append(part.Data);
        }

        var messageCrc = CrcUtil.Crc16(messageBody.ToArray());
        messageBody.Append(messageCrc);
        return messageBody;
    }

    public static PodMessage FromReceivedPackets(List<IPodPacket> receivedPackets)
    {
        var message = new PodMessage();
        var messageBody = new Bytes();
        foreach (var rp in receivedPackets)
            messageBody.Append(rp.Data);

        message.Body = messageBody;

        var crcCalc = CrcUtil.Crc16(messageBody.Sub(0, messageBody.Length - 2).ToArray());
        var messageCrc = messageBody.Word(messageBody.Length - 2);
        if (crcCalc != messageCrc)
            return null;

        message.Address = messageBody.DWord(0);
        var b0 = messageBody[4];
        var b1 = messageBody[5];
        message.WithCriticalFollowup = (b0 & 0x80) > 0;
        message.Sequence = (b0 >> 2) & 0b00111111;
        var bodyLength = ((b0 & 0b00000011) << 8) | b1;
        var bodyIdx = 0;
        message.Parts = new List<IMessagePart>();
        while (bodyIdx < bodyLength)
        {
            var mpType = (PodMessageType)messageBody[6 + bodyIdx];
            Bytes mpData = null;
            var mpLength = 0;
            if (mpType == PodMessageType.ResponseStatus)
            {
                mpLength = messageBody.Length - 2 - 7;
                mpData = messageBody.Sub(6 + bodyIdx + 1, messageBody.Length - 2);
            }
            else
            {
                mpLength = messageBody[6 + bodyIdx + 1];
                mpData = messageBody.Sub(6 + bodyIdx + 2, 6 + bodyIdx + 2 + mpLength);
            }

            switch (mpType)
            {
                case PodMessageType.ResponseStatus:
                    message.Parts.Add(new ResponseStatusPart(mpData));
                    break;
                case PodMessageType.ResponseError:
                    message.Parts.Add(new ResponseErrorPart(mpData));
                    break;
                case PodMessageType.ResponseInfo:
                    var riType = (RequestStatusType)mpData[0];
                    switch (riType)
                    {
                        case RequestStatusType.Alerts:
                            message.Parts.Add(new ResponseInfoAlertsPart(mpData));
                            break;
                        case RequestStatusType.Extended:
                            message.Parts.Add(new ResponseInfoExtendedPart(mpData));
                            break;
                        case RequestStatusType.PulseLogRecent:
                            message.Parts.Add(new ResponseInfoPulseLogRecentPart(mpData));
                            break;
                        case RequestStatusType.Activation:
                            message.Parts.Add(new ResponseInfoActivationPart(mpData));
                            break;
                        case RequestStatusType.PulseLogLast:
                            message.Parts.Add(new ResponseInfoPulseLogLastPart(mpData));
                            break;
                        case RequestStatusType.PulseLogPrevious:
                            message.Parts.Add(new ResponseInfoPulseLogPreviousPart(mpData));
                            break;
                        default:
                            message.Parts.Add(new MessagePart { Data = mpData });
                            break;
                    }

                    break;
                case PodMessageType.ResponseVersionInfo:
                    message.Parts.Add(new ResponseVersionPart(mpData));
                    break;
            }

            bodyIdx = bodyIdx + 2 + mpLength;
        }

        return message;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Address: {Address:X} Sequence: {Sequence} Critical: {WithCriticalFollowup}");
        foreach (var part in Parts)
            sb.Append($"\nPart: {part}");
        return sb.ToString();
    }
}
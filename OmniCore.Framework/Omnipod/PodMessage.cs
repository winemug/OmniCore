using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using Plugin.BLE.Abstractions;

namespace OmniCore.Services;

public class PodMessage : IPodMessage
{
    public uint Address { get; set; }
    public int Sequence { get; set; }
    public bool WithCriticalFollowup { get; set; }
    public IMessageParts Parts { get; set; }

    public uint? AckAddressOverride { get; set; }
    public Bytes Body { get; set; }

    public Bytes GetBody()
    {
        var bodyLength = 0;
        var partsList = Parts.AsList();
        foreach (var part in partsList)
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
        foreach (var part in partsList)
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

    public static PodMessage? FromBody(Bytes messageBody)
    {
        try
        {
            return FromBodyInternal(messageBody);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Message body parsing failed: {e}");
            return null;
        }
    }
    
    private static PodMessage? FromBodyInternal(Bytes messageBody)
    {
        var message = new PodMessage();
        message.Body = messageBody;

        if (messageBody.Length < 8)
            return null;

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

        if (bodyLength != messageBody.Length - 6 - 2)
            return null;
        
        var bodyIdx = 0;
        var partsList = new List<IMessagePart>();
        while (bodyIdx < bodyLength)
        {
            var mpType = (PodMessagePartType)messageBody[6 + bodyIdx];
            Bytes mpData = null;
            var mpLength = 0;
            if (mpType == PodMessagePartType.ResponseStatus)
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
                case PodMessagePartType.ResponseStatus:
                    partsList.Add(new ResponseStatusPart(mpData));
                    break;
                case PodMessagePartType.ResponseError:
                    partsList.Add(new ResponseErrorPart(mpData));
                    break;
                case PodMessagePartType.ResponseInfo:
                    var riType = (RequestStatusType)mpData[0];
                    switch (riType)
                    {
                        case RequestStatusType.Alerts:
                            partsList.Add(new ResponseInfoAlertsPart(mpData));
                            break;
                        case RequestStatusType.Extended:
                            partsList.Add(new ResponseInfoExtendedPart(mpData));
                            break;
                        case RequestStatusType.PulseLogRecent:
                            partsList.Add(new ResponseInfoPulseLogRecentPart(mpData));
                            break;
                        case RequestStatusType.Activation:
                            partsList.Add(new ResponseInfoActivationPart(mpData));
                            break;
                        case RequestStatusType.PulseLogLast:
                            partsList.Add(new ResponseInfoPulseLogLastPart(mpData));
                            break;
                        case RequestStatusType.PulseLogPrevious:
                            partsList.Add(new ResponseInfoPulseLogPreviousPart(mpData));
                            break;
                        default:
                            partsList.Add(new MessagePart { Data = mpData, Type=mpType });
                            break;
                    }
                    break;
                case PodMessagePartType.ResponseVersionInfo:
                    partsList.Add(new ResponseVersionPart(mpData));
                    break;
                default:
                    partsList.Add(new MessagePart { Data = mpData, Type=mpType });
                    break;
            }
            bodyIdx = bodyIdx + 2 + mpLength;
        }

        message.Parts = new MessageParts(partsList);

        return message;

    }
    public static PodMessage? FromReceivedPackets(List<IPodPacket> receivedPackets)
    {
        var messageBody = new Bytes();
        foreach (var rp in receivedPackets)
            messageBody.Append(rp.Data);
        return FromBody(messageBody);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Address: {Address:X} Sequence: {Sequence} Critical: {WithCriticalFollowup}");
        foreach (var part in Parts.AsList())
            sb.Append($"\nPart: {part}");
        return sb.ToString();
    }
}
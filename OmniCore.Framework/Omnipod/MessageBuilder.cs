using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Framework.Omnipod.Messages;
using OmniCore.Framework.Omnipod.Requests;
using OmniCore.Framework.Omnipod.Responses;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;

namespace OmniCore.Framework.Omnipod;

public class MessageBuilder
{
    private class Message : IPodMessage
    {
        public uint Address { get; init; }

        public int Sequence { get; init; }

        public bool Critical { get; init; }

        public Bytes Body { get; init; }

        public IMessageData Data { get; init; }
    }

    public uint? Address { get; private set; }
    public int? Sequence { get; private set; }
    public bool Critical { get; private set; }
    public INonceProvider? NonceProvider { get; private set; }
    public IMessageData? Data { get; private set; }
    public Bytes? Body { get; private set; }

    public static Predicate<IMessageParts> CanParse => throw new NotImplementedException();

    public MessageBuilder WithAddress(uint address)
    {
        Address = address;
        return this;
    }

    public MessageBuilder WithSequence(int sequence)
    {
        Sequence = sequence;
        return this;
    }

    public MessageBuilder WithNonceProvider(INonceProvider nonceProvider)
    {
        NonceProvider = nonceProvider;
        return this;
    }

    public MessageBuilder AsCritical()
    {
        Critical = true;
        return this;
    }

    public IPodMessage Build(Bytes messageBody)
    {
        if (messageBody.Length < 8)
            throw new ApplicationException();

        var crcCalc = CrcUtil.Crc16(messageBody.Sub(0, messageBody.Length - 2).ToArray());
        var messageCrc = messageBody.Word(messageBody.Length - 2);
        if (crcCalc != messageCrc)
            throw new ApplicationException();

        var address = messageBody.DWord(0);
        var b0 = messageBody[4];
        var b1 = messageBody[5];
        var withCriticalFollowup = (b0 & 0x80) > 0;
        var sequence = b0 >> 2 & 0b00111111;
        var bodyLength = (b0 & 0b00000011) << 8 | b1;

        if (bodyLength != messageBody.Length - 6 - 2)
            throw new ApplicationException();

        var bodyIdx = 0;
        var partsList = new List<IMessagePart>();
        while (bodyIdx < bodyLength)
        {
            var DType = (PodMessagePartType)messageBody[6 + bodyIdx];
            Bytes mpData = null;
            var mpLength = 0;
            if (DType == PodMessagePartType.ResponseStatus)
            {
                mpLength = messageBody.Length - 2 - 7;
                mpData = messageBody.Sub(6 + bodyIdx + 1, messageBody.Length - 2);
            }
            else
            {
                mpLength = messageBody[6 + bodyIdx + 1];
                mpData = messageBody.Sub(6 + bodyIdx + 2, 6 + bodyIdx + 2 + mpLength);
            }

            partsList.Add(new MessagePart { Data = mpData, Type = DType });
            bodyIdx = bodyIdx + 2 + mpLength;
        }

        var parts = new MessageParts(partsList);

        IMessageData? messageData = null;

        messageData ??= TryParse<StatusMessage>(parts);
        messageData ??= TryParse<NonceSyncMessage>(parts);
        messageData ??= TryParse<StatusExtendedMessage>(parts);
        messageData ??= TryParse<ErrorMessage>(parts);
        messageData ??= TryParse<AlertInfoMessage>(parts);
        messageData ??= TryParse<InitializationInfoMessage>(parts);
        messageData ??= TryParse<VersionMessage>(parts);
        messageData ??= TryParse<VersionExtendedMessage>(parts);

        messageData ??= TryParse<AcknowledgeAlertsMessage>(parts);
        messageData ??= TryParse<AssignAddressMessage>(parts);
        messageData ??= TryParse<DeactivateMessage>(parts);
        messageData ??= TryParse<GetStatusMessage>(parts);
        messageData ??= TryParse<SetBeepingMessage>(parts);
        messageData ??= TryParse<SetAlertsMessage>(parts);
        messageData ??= TryParse<SetClockMessage>(parts);
        messageData ??= TryParse<SetDeliveryVerificationMessage>(parts);
        messageData ??= TryParse<StartBasalMessage>(parts);
        messageData ??= TryParse<StartBolusMesage>(parts);
        messageData ??= TryParse<StartTempBasalMessage>(parts);
        messageData ??= TryParse<StopDeliveryMessage>(parts);

        if (messageData == null)
            throw new ApplicationException();

        return new Message
        {
            Body = messageBody,
            Data = messageData,
            Address = address,
            Sequence = sequence,
            Critical = withCriticalFollowup,
        };
    }

    private static IMessageData? TryParse<T>(IMessageParts parts) where T : IMessageData, new()
    {
        if (T.CanParse(parts))
            return new T().FromParts(parts);
        return null;
    }

    public IPodMessage Build(IMessageData messageData)
    {
        if (!Address.HasValue || !Sequence.HasValue)
            throw new ApplicationException("Address and sequence must be provided");

        var parts = messageData.ToParts();
        var msgParts = new List<IMessagePart>();
        var partsList = parts.AsList();
        var bodyLength = 0;
        foreach (var part in partsList)
        {
            if (part.RequiresNonce)
            {
                if (NonceProvider == null)
                    throw new ApplicationException("This message requires a nonce provider");
                part.Nonce = NonceProvider.NextNonce();
                bodyLength += 4;
            }
            bodyLength += part.Data.Length + 2;
            msgParts.Add(part);
        }

        var messageBody = new Bytes(Address.Value);
        byte b0 = 0x00;
        if (Critical)
            b0 = 0x80;
        b0 |= (byte)(Sequence << 2);
        b0 |= (byte)(bodyLength >> 8 & 0x03);
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
        return new Message
        {
            Body = messageBody,
            Data = messageData,
            Address = this.Address.Value,
            Sequence = this.Sequence.Value,
            Critical = this.Critical,
        };
    }
}

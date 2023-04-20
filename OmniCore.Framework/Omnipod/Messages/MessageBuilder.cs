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

namespace OmniCore.Framework.Omnipod.Messages;

public class MessageBuilder
{
    public uint? Address { get; private set; }
    public int? Sequence { get; private set; }
    public bool Critical { get; private set; }
    public INonceProvider? NonceProvider { get; private set; }
    public IMessageData? Data { get; private set; }
    public Bytes? Body { get; private set; }

    public static Predicate<IMessageParts> CanParse => throw new NotImplementedException();

    public MessageBuilder WithAddress(uint address)
    {
        this.Address = address;
        return this;
    }

    public MessageBuilder WithSequence(int sequence)
    {
        this.Sequence = sequence;
        return this;
    }

    public MessageBuilder WithNonceProvider(INonceProvider nonceProvider)
    {
        this.NonceProvider = nonceProvider;
        return this;
    }

    public MessageBuilder AsCritical()
    {
        this.Critical = true;
        return this;
    }

    public MessageBuilder WithBody(Bytes messageBody)
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
        var sequence = (b0 >> 2) & 0b00111111;
        var bodyLength = ((b0 & 0b00000011) << 8) | b1;

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
        messageData ??= TryParse<AckAlertsMessage>(parts);
        messageData ??= TryParse<BeepMessage>(parts);
        messageData ??= TryParse<CancelMessage>(parts);

        if (messageData == null)
            throw new ApplicationException();
        Body = messageBody;
        Data = messageData;
        Address = address;
        Sequence = sequence;
        Critical = withCriticalFollowup;
        return this;

    }

    private static IMessageData? TryParse<T>(IMessageParts parts) where T : IMessageData, new()
    {
        if (T.CanParse(parts))
            return new T().FromParts(parts);
        return null;
    }

    public MessageBuilder WithData(IMessageData messageData)
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
                if (this.NonceProvider == null)
                    throw new ApplicationException("This message requires a nonce provider");
                part.Nonce = this.NonceProvider.NextNonce();
                bodyLength += 4;
            }
            bodyLength += part.Data.Length + 2;
            msgParts.Add(part);
        }

        var messageBody = new Bytes(this.Address.Value);
        byte b0 = 0x00;
        if (this.Critical)
            b0 = 0x80;
        b0 |= (byte)(this.Sequence << 2);
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
        Body = messageBody;
        Data = messageData;

        return this;
    }
}

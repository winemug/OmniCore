using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Messages;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System.Reflection;

namespace OmniCore.Framework.Omnipod.Parts;

public abstract class Message<T> : IMessage<T> where T : IMessageData
{
    public uint Address { get; private set; }
    public int Sequence { get; private set; }
    public bool Critical { get; private set; }
    public INonceProvider? NonceProvider { get; private set; }
    public Bytes? ByteData { get; private set; }
    public T? MessageData { get; private set; }

    public abstract IMessageParts DataToParts(T md);
    public abstract T PartsToData(IMessageParts parts);

    public IMessage<T> WithMessageData(T messageData)
    {
        ByteData = BodyFromParameters(messageData);
        MessageData = messageData;
        return this;
    }

    public IMessage<T> WithAddress(uint address)
    {
        this.Address = address;
        return this;
    }

    public IMessage<T> WithSequence(int sequence)
    {
        this.Sequence = sequence;
        return this;
    }

    public IMessage<T> WithNonceProvider(INonceProvider nonceProvider)
    {
        this.NonceProvider = nonceProvider;
        return this;
    }

    public IMessage<T> AsCritical()
    {
        this.Critical = true;
        return this;
    }

    private Bytes BodyFromParameters(T mp)
    {
        var parts = DataToParts(mp);
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

        var messageBody = new Bytes(this.Address);
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
        return messageBody;
    }

    private static IMessageData FromBody(Bytes body)
    {
        if (body.Length < 8)
            throw new ApplicationException();

        var crcCalc = CrcUtil.Crc16(body.Sub(0, body.Length - 2).ToArray());
        var messageCrc = body.Word(body.Length - 2);
        if (crcCalc != messageCrc)
            throw new ApplicationException();

        var address = body.DWord(0);
        var b0 = body[4];
        var b1 = body[5];
        var withCriticalFollowup = (b0 & 0x80) > 0;
        var sequence = (b0 >> 2) & 0b00111111;
        var bodyLength = ((b0 & 0b00000011) << 8) | b1;

        if (bodyLength != body.Length - 6 - 2)
            throw new ApplicationException();

        var bodyIdx = 0;
        var partsList = new List<IMessagePart>();
        while (bodyIdx < bodyLength)
        {
            var DType = (PodMessagePartType)body[6 + bodyIdx];
            Bytes mpData = null;
            var mpLength = 0;
            if (DType == PodMessagePartType.ResponseStatus)
            {
                mpLength = body.Length - 2 - 7;
                mpData = body.Sub(6 + bodyIdx + 1, body.Length - 2);
            }
            else
            {
                mpLength = body[6 + bodyIdx + 1];
                mpData = body.Sub(6 + bodyIdx + 2, 6 + bodyIdx + 2 + mpLength);
            }

            partsList.Add(new MessagePart { Data = mpData, Type = DType });
            bodyIdx = bodyIdx + 2 + mpLength;
        }

        var parts = new MessageParts(partsList);

        Message<IMessageData>? parsedMessage = null;

        switch (parts.MainPart.Type)
        {
            case PodMessagePartType.RequestBeepConfig:
                parsedMessage = new BeepMessage();
                break;
        }

        if (parsedMessage == null)
            return null;

        var mp = parsedMessage.PartsToData(parts);
        this.Address = address;
        this.Sequence = sequence;
        this.Critical = withCriticalFollowup;
        return mp;
    }
}

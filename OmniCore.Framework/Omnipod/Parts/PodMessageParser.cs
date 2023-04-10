using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public static class PodMessageParser
{
    public static PodMessage FromBody(Bytes messageBody)
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
            partsList.Add(new MessagePart { Data = mpData, Type=mpType });
            bodyIdx = bodyIdx + 2 + mpLength;
        }

        message.Parts = new MessageParts(partsList);
        return message;
    }
}
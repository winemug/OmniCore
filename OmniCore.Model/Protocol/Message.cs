using OmniCore.Model.Protocol.Base;
using OmniCore.Model.Protocol.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OmniCore.Model.Protocol
{
    public class Message : IMessage
    {
        private List<IMessagePart> _parts;

        protected Message()
        {
        }

        public bool WillFollowUpWithCriticalRequest { get; protected set; }

        public byte[] GetMessageData()
        {
            int sumParts = this.Parts.Sum(x => x.Data.Length);
            var msgData = new byte[sumParts + 2];
            int dstPtr = 0;
            foreach(var part in Parts)
            {
                Buffer.BlockCopy(part.Data, 0, msgData, dstPtr, part.Data.Length);
                dstPtr += part.Data.Length;
            }
            var crc16 = CrcUtil.Crc16(msgData, msgData.Length - 2);
            msgData[msgData.Length - 2] = (byte)((crc16 & 0xFF00) >> 8);
            msgData[msgData.Length - 1] = (byte)(crc16 & 0x00FF);
            return msgData;
        }

        public static Message Parse(byte[] messageData)
        {
            if (messageData == null || messageData.Length == 0)
                throw new ParserException("Message is empty");

            var parts = new List<IMessagePart>();
            int msgptr = 0;
            while (msgptr < messageData.Length)
            {
                int partDataLength = 0;
                var partType = messageData[msgptr++];
                var hasLengthField = true;
                if (partType == 0x0d)
                {
                    hasLengthField = false;
                    partDataLength = messageData.Length - msgptr - 2;
                }
                else
                {
                    if (msgptr > messageData.Length)
                        throw new ParserException("Partial message too short");

                    partDataLength = messageData[msgptr++];
                }

                if (msgptr + partDataLength + 2 > messageData.Length)
                    throw new ParserException("Partial message too short");

                var partData = new byte[partDataLength];
                Buffer.BlockCopy(partData, msgptr, partData, 0, partDataLength);
                msgptr += partDataLength;

                uint crc16 = messageData[msgptr++];
                crc16 = crc16 << 8;
                crc16 |= messageData[msgptr++];

                var generatedCrc = CrcUtil.Crc16(partData);
                if (generatedCrc != crc16)
                    throw new ParserException("Partial message crc error");

                parts.Add(new MessagePart(hasLengthField, false, partType, partData));
            }

            switch(parts[0].PartType)
            {
                case 0x02:
                    return new InformationResponse(parts);
                case 0x0d:
                    return new StatusResponse(parts);
                default:
                    throw new ParserException("Unknown message type");
            }
        }

        public IEnumerable<IMessagePart> Parts
        {
            get
            {
                return _parts;
            }
            protected set
            {
                _parts = new List<IMessagePart>();
                var previousPartHasLengthField = true;
                foreach (var part in value)
                {
                    if (!previousPartHasLengthField)
                        throw new ParserException("Cannot create a new part after a part with no length field");
                    _parts.Add(part);
                    previousPartHasLengthField = part.HasLengthField;
                }
            }
        }
    }
}

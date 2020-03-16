using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Utilities;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkErosConversation
    {
        public int PacketSequence { get; private set; }

        private List<Bytes> RequestPacketData;
        private Bytes ResponseData;
        private int SendPacketIndex = 0;
        private byte[] SendPacketCache = null;
        public bool IsFinished = false;

        private readonly IErosPod Pod;

        private IErosPodRequest Request;

        private static Dictionary<long, RileyLinkErosConversation> ConversationCache =
            new Dictionary<long, RileyLinkErosConversation>();

        public static RileyLinkErosConversation ForPod(IErosPod erosPod)
        {
            if (!ConversationCache.ContainsKey(erosPod.Entity.Id))
            {
                ConversationCache.Add(erosPod.Entity.Id, new RileyLinkErosConversation(erosPod));
            }

            return ConversationCache[erosPod.Entity.Id];
        }

        private RileyLinkErosConversation(IErosPod pod)
        {
            PacketSequence = 0;
            Pod = pod;
        }

        public void Initialize(IErosPodRequest request)
        {
            Request = request;
            var requestData = new Bytes(request.Message);
            RequestPacketData = new List<Bytes>();
            int index = 0;
            while (index < requestData.Length)
            {
                var packetBodyLength = Math.Min(31, requestData.Length - index);
                RequestPacketData.Add(requestData.Sub(index, index + packetBodyLength));
            }

            ResponseData = new Bytes();
        }

        public byte[] GetPacketToSend()
        {
            if (SendPacketCache == null)
            {
                RadioPacket rpSend;
                if (SendPacketIndex == 0)
                {
                    rpSend = new RadioPacket(Request.MessageRadioAddress, PacketSequence, PacketType.PDM, RequestPacketData[0]);
                }
                else if (SendPacketIndex < RequestPacketData.Count)
                {
                    rpSend = new RadioPacket(Request.MessageRadioAddress, PacketSequence, PacketType.CON,
                        RequestPacketData[SendPacketIndex]);
                }
                else if (IsFinished)
                {
                    rpSend = new RadioPacket(Request.MessageRadioAddress, PacketSequence, PacketType.ACK,
                        new Bytes((uint)0));
                }
                else
                {
                    rpSend = new RadioPacket(Request.MessageRadioAddress, PacketSequence, PacketType.ACK,
                        new Bytes(Request.MessageRadioAddress));
                }

                SendPacketCache = rpSend.GetPacketData();
            }
            return SendPacketCache;
        }

        public bool ParseIncomingPacket(byte[] incomingPacketData)
        {
            if (IsFinished)
            {
                if (incomingPacketData == null || incomingPacketData.Length == 0)
                    return true;
                else
                    return false;
            }
            
            var incomingRadioPacket = RadioPacket.FromIncoming(incomingPacketData);
            if (!incomingRadioPacket.IsValid)
                return false;

            var receivePacketSequence = (PacketSequence + 1) % 32;

            if (incomingRadioPacket.Sequence != receivePacketSequence)
                return false;

            if (SendPacketIndex < RequestPacketData.Count)
            {
                if (incomingRadioPacket.Type != PacketType.ACK)
                    return false;
            }
            else
            {
                if (SendPacketIndex == RequestPacketData.Count)
                {
                    if (incomingRadioPacket.Type != PacketType.POD)
                        return false;
                }
                else
                {
                    if (incomingRadioPacket.Type != PacketType.CON)
                        return false;
                    ResponseData.Append(incomingRadioPacket.Data);
                }

                var responseDataCandidate = new Bytes(ResponseData);
                responseDataCandidate.Append(incomingRadioPacket.Data);
                var evaluation = EvaluateResponse(responseDataCandidate);
                if (evaluation.IsValid)
                {
                    ResponseData = responseDataCandidate;
                    IsFinished = evaluation.IsComplete;
                }
                else
                {
                    return false;
                }
            }

            SendPacketIndex++;
            SendPacketCache = null;
            PacketSequence = (receivePacketSequence + 1) % 32;

            return true;
        }

        private (bool IsValid, bool IsComplete) EvaluateResponse(Bytes data)
        {
            bool isValid = true;
            bool isComplete = false;

            if (data.Length >= 4)
            {
                var radioAddress = data.DWord(0);
                if (!Request.AllowAddressOverride && radioAddress != Request.MessageRadioAddress)
                    isValid = false;
            }

            if (data.Length >= 6)
            {
                var messageExpectedLength = data.Word(4) & 0x3FF;

                var responseExpectedLength = 4 + 2 + messageExpectedLength + 2;
                if (data.Length > responseExpectedLength)
                {
                    isValid = false;
                }
                else if (data.Length == responseExpectedLength)
                {
                    var crc = data.Word(data.Length - 2);
                    var crcCalculated = CrcUtil.Crc16(data, data.Length -2);

                    if (crc == crcCalculated)
                    {
                        isComplete = true;
                    }
                    else
                    {
                        isValid = false;
                    }
                }
            }

            return (isValid, isComplete);
        }
    }

    class RadioPacket
    {
        public int? Sequence { get; set; }
        public uint? Address { get; set; }
        public PacketType? Type { get; set; }
        public Bytes Data { get; set; }
        public byte? Crc { get; set; }
        public bool IsValid { get; set; }

        private RadioPacket()
        {
            IsValid = false;
        }

        public RadioPacket(uint address, int sequence, PacketType type, Bytes data)
        {
            Address = address;
            Sequence = sequence;
            Type = type;
            Data = data;
        }

        public byte[] GetPacketData()
        {
            if (!Address.HasValue || !Sequence.HasValue || !Type.HasValue)
                return null;

            var packetData = new Bytes(Address.Value);
            var d4 = ((int) Type.Value << 5) | (Sequence.Value & 0b00011111);
            packetData.Append((byte)d4);
            if (Data != null)
                packetData.Append(Data);
            Crc = CrcUtil.Crc8(packetData);
            packetData.Append(Crc.Value);
            return ManchesterEncoding.Encode(packetData).ToArray();
        }

        public static RadioPacket FromIncoming(byte[] incomingData)
        {
            int? sequence = null;
            uint? address = null;
            PacketType? type = null;
            Bytes data = null;
            byte? crc = null;

            var incoming = ManchesterEncoding.Decode(new Bytes(incomingData));

            if (incoming.Length >= 4)
            {
                address = incoming.DWord(0);
            }

            if (incoming.Length >= 5)
            {
                var d4 = incoming[4];
                type = (PacketType) (d4 >> 5);
                sequence = d4 & 0b00011111;
            }

            if (incoming.Length >= 6)
            {
                crc = incoming[incoming.Length - 1];
            }

            if (incoming.Length >= 7)
            {
                data = incoming.Sub(5, incoming.Length - 1);
            }

            bool isValid = true;
            if (crc.HasValue)
            {
                var computedCrc = CrcUtil.Crc8(incoming, incoming.Length);
                if (crc != computedCrc)
                    isValid = false;
            }
            else
                isValid = false;

            switch (type)
            {
                case PacketType.Unknown_000:
                case PacketType.Unknown_001:
                case PacketType.Unknown_110:
                case PacketType.Unknown_011:
                case null:
                    isValid = false;
                    break;
                case PacketType.ACK:
                    if (data == null || data.Length != 4)
                        isValid = false;
                    break;
                case PacketType.CON:
                    if (data == null || data.Length == 0)
                        isValid = false;
                    break;
                case PacketType.PDM:
                case PacketType.POD:
                    if (data == null || data.Length < 8)
                        isValid = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new RadioPacket
            {
                Address = address,
                Sequence = sequence,
                Type = type,
                Data = data,
                Crc = crc,
                IsValid = isValid
            };
        }
    }

    static class ManchesterEncoding
    {
        private static readonly ushort[] Encoded;
        private static Dictionary<ushort, byte> Decoded;

        private static byte[] Noise;
        private static Random Rnd;

        static ManchesterEncoding()
        {
            Encoded = new ushort[256];
            Decoded = new Dictionary<ushort, byte>();

            var encoding0 = new ushort[8];
            var encoding1 = new ushort[8];
            var mask = new byte[8];

            for (int b = 0; b < 8; b++)
            {
                mask[b] = (byte) (1 << b);
                encoding0[b] = (ushort) (2 << (b * 2));
                encoding1[b] = (ushort) (1 << (b * 2));
            }

            for (int dec = 0; dec < 256; dec++)
            {
                ushort enc = 0;
                for (int b = 0; b < 8; b++)
                {
                    if ((dec & mask[b]) == 0)
                        enc |= encoding0[b];
                    else
                        enc |= encoding1[b];
                }

                var ebHi = (byte) ((enc & 0xFF00) >> 8);
                var ebLo = (byte) (enc & 0x00FF);
                Encoded[dec] = enc;
                Decoded.Add(enc, (byte) dec);
            }

            Rnd = new Random();
            Noise = new byte[256 + 160];
            for (int i = 0; i < Noise.Length; i++)
            {
                byte noise = 0;
                for (int j = 0; j < 4; j++)
                {
                    noise = (byte) (noise << 2);
                    if (Rnd.Next() % 2 == 0)
                        noise |= 0x00;
                    else
                        noise |= 0x03;
                }

                Noise[i] = noise;
            }
        }

        public static Bytes Encode(Bytes toEncode)
        {
            var encoded = new Bytes();
            int noiseIndex = Rnd.Next(0, 256);
            for (int i = 0; i < 40; i++)
            {
                if (i < toEncode.Length)
                {
                    var byteToEncode = toEncode[i];
                    encoded.Append(Encoded[byteToEncode]);
                }
                else
                {
                    encoded.Append(Noise[noiseIndex + i * 2]);
                    encoded.Append(Noise[noiseIndex + (i * 2 + 1)]);
                }
            }

            return encoded;
        }

        public static Bytes Decode(Bytes toDecode)
        {
            var decoded = new Bytes();
            for (int i = 0; i < toDecode.Length; i += 2)
            {
                var wordToDecode = toDecode.Word(i);
                if (Decoded.ContainsKey(wordToDecode))
                {
                    decoded.Append(Decoded[wordToDecode]);
                }
                else
                {
                    break;
                }
            }

            return decoded;
        }
    }
}

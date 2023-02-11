using System;
using System.Diagnostics;
using System.Linq;
using OmniCore.Services.Interfaces;
using Plugin.BLE.Abstractions.Extensions;

namespace OmniCore.Services
{
    public class RadioPacket
    {
        public int? Rssi { get; set; }
        public uint Address { get; set; }

        public RadioPacketType Type { get; set; }
        
        public int Sequence { get; set; }
        
        public Bytes Data { get; set; }

        public RadioPacket(uint address, RadioPacketType type, int sequence, Bytes data)
        {
            Address = address;
            Type = type;
            Sequence = sequence;
            Data = data;
        }

        public byte[] ToRadioData()
        {
            var dataLen = 6;
            if (Data != null)
                dataLen += Data.Length;

            var radioData = new byte[dataLen];
            radioData[0] = (byte)((Address >> 24) & 0xFF);
            radioData[1] = (byte)((Address >> 16) & 0xFF);
            radioData[2] = (byte)((Address >> 8) & 0xFF);
            radioData[3] = (byte)((Address) & 0xFF);

            byte b4 = (byte)(Sequence & 0b00011111);
            switch (Type)
            {
                case RadioPacketType.Ack:
                    b4 |= 0b01000000;
                    break;
                case RadioPacketType.Con:
                    b4 |= 0b10000000;
                    break;
                case RadioPacketType.Pdm:
                    b4 |= 0b10100000;
                    break;
                case RadioPacketType.Pod:
                    b4 |= 0b11100000;
                    break;
                default:
                    throw new ApplicationException("Unknown packet type");
            }

            radioData[4] = b4;
            if (Data != null && Data.Length > 0)
                Data.ToArray().CopyTo(radioData, 5);

            var crc = CrcUtil.Crc8(new ArraySegment<byte>(radioData, 0, dataLen - 1).ToArray());
            radioData[dataLen - 1] = crc;

            // Debug.WriteLine($"RAWO: {radioData.ToHexString()}");
            // return radioData;
            return ManchesterCodec.Encode(radioData);
        }
        
        public static RadioPacket FromRadioData(Bytes data, int radioRssi)
        {
            data = new Bytes(ManchesterCodec.Decode(data.ToArray()));
            if (data.Length < 6)
                return null;
            // throw new ApplicationException("Invalid packet");

            var crcByte = data[data.Length - 1];
            var crc = CrcUtil.Crc8(data.Sub(0, data.Length - 1).ToArray());

            if (crc != crcByte)
                return null;
            // throw new ApplicationException("Invalid crc");

            // Debug.WriteLine($"RAWI: {data.ToHexString()}");
            var address = (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
            RadioPacketType type;
            switch (data[4] & 0b11100000)
            {
                case 0b01000000:
                    type = RadioPacketType.Ack;
                    break;
                case 0b10000000:
                    type = RadioPacketType.Con;
                    break;
                case 0b10100000:
                    type = RadioPacketType.Pdm;
                    break;
                case 0b11100000:
                    type = RadioPacketType.Pod;
                    break;
                default:
                    return null;
                // throw new ApplicationException("Unknown packet type");
            }

            int sequence = data[4] & 0b00011111;

            return new RadioPacket(address, type, sequence, data.Sub(5, data.Length - 1))
            {
                Rssi = radioRssi,
            };
        }

        public override string ToString()
        {
            return $"Address: {Address:X} Type: {Type} Seq: {Sequence} Data: {Data}" +
                   (Rssi.HasValue ? $" RSSI: {Rssi}" : "");
        }
    }
}
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;

namespace OmniCore.Radio.RileyLink
{
    public class RadioPacket
    {
        public uint Address;
        public PacketType Type;
        public int Sequence;
        public Bytes Body;
        public sbyte Rssi;

        public Bytes PartialData
        {
            get
            {
                return GetPacketData();
            }
        }

        public RadioPacket(uint address, PacketType type, int sequence, Bytes body)
        {
            this.Address = address;
            this.Type = type;
            this.Sequence = sequence % 32;
            this.Body = body;
        }

        public static RadioPacket Parse(Bytes data)
        {
            if (data.Length < 5)
                return null;

            var crc_computed = CrcUtil.Crc8(data.Sub(0, data.Length - 1).ToArray());
            var crc = data[data.Length - 1];
            if (crc != crc_computed)
                return null;

            var address = data.DWord(0);
            var d4 = data.Byte(4);
            var type = (PacketType)(d4 >> 5);
            var sequence = d4 & 0b00011111;
            var body = data.Sub(5, data.Length - 1);
            return new RadioPacket(address, type, sequence, body);
        }

        public RadioPacket WithSequence(int sequence)
        {
            this.Sequence = sequence;
            return this;
        }

        public Bytes GetPacketData()
        {
            var data = new Bytes().Append(this.Address);
            data.Append((byte)(((int)this.Type << 5) | this.Sequence));
            data.Append(this.Body);
            data.Append(CrcUtil.Crc8(data.ToArray()));
            return data;
        }

        public override string ToString()
        {
            if (this.Type == PacketType.CON)
            {
                return $"0x{this.Sequence:X2} {this.Type.ToString().Substring(0, 3)} 0x{this.Address:X8} {this.Body.ToHex()}";
            }
            else
            {
                return $"0x{this.Sequence:X2} {this.Type.ToString().Substring(0, 3)} 0x{this.Address:X8} 0x{this.Body.ToHex(0, 4)} {this.Body.ToHex(4)}";
            }
        }
    }
}


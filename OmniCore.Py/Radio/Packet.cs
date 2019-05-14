using OmniCore.py;

namespace OmniCore.Py
{
    public class Packet
    {
        public uint address;
        public RadioPacketType type;
        public int sequence;
        public Bytes body;
        public byte rssi;

        public Packet(uint address, RadioPacketType type, int sequence, Bytes body)
        {
            this.address = address;
            this.type = type;
            this.sequence = sequence % 32;
            this.body = body;
        }

        public static Packet parse(Bytes data)
        {
            if (data.Length < 5)
                return null;

            var crc_computed = CrcUtil.Crc8(data.Sub(0, data.Length - 1).ToArray());
            var crc = data[data.Length - 1];
            if (crc != crc_computed)
                return null;

            var address = data.DWord(0);
            var d4 = data.Byte(4);
            var type = (RadioPacketType)(d4 & 0b11100000);
            var sequence = d4 & 0b00011111;
            var body = data.Sub(5);
            return new Packet(address, type, sequence, body);
        }

        public Packet with_sequence(int sequence)
        {
            this.sequence = sequence;
            return this;
        }

        public Bytes get_data()
        {
            var data = new Bytes().Append(this.address);
            data.Append((byte)((int)this.type | this.sequence));
            data.Append(this.body);
            data.Append(CrcUtil.Crc8(data.ToArray()));
            return data;
        }

        public override string ToString()
        {
            if (this.type == RadioPacketType.CON)
            {
                return $"0x{this.sequence:X2} {this.type.ToString().Substring(0, 3)} 0x{this.address:X8} {this.body.ToHex()}";
            }
            else
            {
                return $"0x{this.sequence:X2} {this.type.ToString().Substring(0, 3)} 0x{this.address:X8} 0x{this.body.ToHex(0, 4)} {this.body.ToHex(4)}";
            }
        }
    }
}


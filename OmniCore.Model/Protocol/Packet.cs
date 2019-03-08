using OmniCore.Model.Protocol.Base;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol
{
    public class Packet
    {
        public uint Address { get; set; }
        public uint? Address2 { get; set; }
        public int Sequence { get; set; }
        public PacketType Type { get; set; }
        public byte[] Body { get; private set; }

        public Packet()
        {
        }

        public Packet(byte[] data)
        {
            this.Address = data.GetUInt32BigEndian(0);
            this.Type = (PacketType) (data[4] >> 5);
            this.Sequence = data[4] & 0x1F;

            if (this.Type == PacketType.PDM || this.Type == PacketType.POD || this.Type == PacketType.ACK)
            {
                this.Address2 = data.GetUInt32BigEndian(5);
                if (data.Length > 9)
                {
                    this.Body = new byte[data.Length - 9];
                    Buffer.BlockCopy(data, 9, this.Body, 0, this.Body.Length);
                }
            }
            else
            {
                if (data.Length > 5)
                {
                    this.Body = new byte[data.Length - 5];
                    Buffer.BlockCopy(data, 5, this.Body, 0, this.Body.Length);
                }
            }
        }

        public override string ToString()
        {
            return $"AD1: {this.Address:%8X}";
        }
    }
}

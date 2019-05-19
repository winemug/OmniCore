using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;

namespace OmniCore.Model
{
    public class Message : IMessage
    {
        public uint? address { get; set; }
        public int? sequence { get; set; }
        public bool expect_critical_followup { get; set; }
        public int body_length { get; set; }
        public Bytes body { get; set; }
        public Bytes body_prefix { get; set; }
        public List<Tuple<byte, Bytes, uint?>> parts { get; set; }
        public string message_str_prefix { get; set; }
        public PacketType? type { get; set; }
        public TxPower? TxLevel { get; set; }
        public uint? AckAddressOverride { get; set; }
        public bool DoubleTake { get; set; }

        public bool add_radio_packet(Packet radio_packet)
        {
            if (radio_packet.type == PacketType.POD || radio_packet.type == PacketType.PDM)
            {
                this.type = radio_packet.type;
                this.address = radio_packet.body.DWord(0);
                var r4 = radio_packet.body.Byte(4);
                this.sequence = (r4 >> 2) & 0x0f;
                this.expect_critical_followup = (r4 & 0x80) > 0;
                this.body_length = ((r4 & 0x03) << 8) | radio_packet.body.Byte(5);
                this.body_prefix = radio_packet.body.Sub(0, 5);
                this.body = radio_packet.body.Sub(5);
            }
            else
            {
                if (radio_packet.type == PacketType.CON)
                    this.body.Append(radio_packet.body);
                else
                    throw new ProtocolException("Packet type invalid");
            }

            if (this.body_length == this.body.Length - 2)
            {
                var bodyWithoutCrc = this.body.Sub(0, this.body.Length - 2);
                var crc = this.body.DWord(this.body.Length - 2);
                var crc_calculated = CrcUtil.Crc16(new Bytes(this.body_prefix, bodyWithoutCrc).ToArray());

                if (crc == crc_calculated)
                {
                    this.body = bodyWithoutCrc;
                    var bi = 0;
                    while (bi < this.body.Length)
                    {
                        var response_type = this.body[bi];
                        Bytes response_body;
                        if (response_type == 0x1d)
                        {
                            response_body = this.body.Sub(bi + 1);
                            bi = this.body.Length;
                        }
                        else
                        {
                            var response_len = this.body[bi + 1];
                            response_body = this.body.Sub(bi + 2, bi + 2 + response_len);
                            bi += response_len + 2;
                        }
                        this.parts.Add(new Tuple<byte, Bytes, uint?>(response_type, response_body, null));
                    }
                    return true;
                }
                else
                {
                    throw new ProtocolException("Message crc error");
                }
            }
            else
            {
                return false;
            }
        }

        public List<Packet> get_radio_packets(int first_packet_sequence)
        {
            this.message_str_prefix = $"{this.address:%08X} {this.sequence:%02X} {this.expect_critical_followup} ";

            var message_body_len = 0;
            foreach (var p in this.parts)
            {
                var cmd_body = p.Item2;
                var nonce = p.Item3;
                message_body_len += (int)cmd_body.Length + 2;
                if (nonce != null)
                    message_body_len += 4;
            }

            byte b0 = 0;
            if (this.expect_critical_followup)
                b0 = 0x80;

            b0 |= (byte)(this.sequence << 2);
            b0 |= (byte)((message_body_len >> 8) & 0x03);
            byte b1 = (byte)(message_body_len & 0xff);

            var message_body = new Bytes(this.address.Value);
            message_body.Append(b0);
            message_body.Append(b1);

            foreach (var p in this.parts)
            {
                var cmd_type = p.Item1;
                var cmd_body = p.Item2;
                var nonce = p.Item3;

                if (nonce == null)
                {
                    if (cmd_type == (byte)PodResponse.Status)
                        message_body.Append(cmd_type);
                    else
                    {
                        message_body.Append(cmd_type);
                        message_body.Append((byte)cmd_body.Length);
                    }
                }
                else
                {
                    message_body.Append(cmd_type);
                    message_body.Append((byte)(cmd_body.Length + 4));
                    message_body.Append(nonce.Value);
                }
                message_body.Append(cmd_body[0]);
            }
            var crc_calculated = CrcUtil.Crc16(message_body.ToArray());
            message_body.Append(crc_calculated);

            int index = 0;
            bool first_packet = true;
            int sequence = first_packet_sequence;
            int total_body_len = (int)message_body.Length;
            var radio_packets = new List<Packet>();

            while (index < message_body.Length)
            {
                var to_write = Math.Min(31, message_body.Length - index);
                var packet_body = message_body.Sub(index, index + to_write);
                radio_packets.Add(new Packet(AckAddressOverride.Value,
                                                first_packet ? this.type.Value : PacketType.CON,
                                                sequence,
                                                packet_body));
                first_packet = false;
                sequence = (sequence + 2) % 32;
                index += to_write;
            }

            if (this.DoubleTake)
            {
                var fp = radio_packets[0];
                radio_packets.Insert(0, fp);
            }
            return radio_packets;
        }

        public void add_part(PdmRequest cmd_type, Bytes cmd_body)
        {
            this.parts.Add(new Tuple<byte, Bytes, uint?>((byte)cmd_type, cmd_body, null));
        }
    }
}


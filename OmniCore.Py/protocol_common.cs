using Omni.Py;
using OmniCore.py;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Py
{
    public enum PdmRequest
    {
        SetupPod = 0x03,
        AssignAddress = 0x07,
        SetDeliveryFlags = 0x08,
        Status = 0x0e,
        AcknowledgeAlerts = 0x11,
        BasalSchedule = 0x13,
        TempBasalSchedule = 0x16,
        BolusSchedule = 0x17,
        ConfigureAlerts = 0x19,
        InsulinSchedule = 0x1a,
        DeactivatePod = 0x1c,
        CancelDelivery = 0x1f
    }

    public enum PodResponse
    {
        VersionInfo = 0x01,
        DetailInfo = 0x02,
        ResyncRequest = 0x06,
        Status = 0x1d
    }

    public enum RadioPacketType
    {
        UN0 = 0b00000000,
        UN1 = 0b00100000,
        ACK = 0b01000000,
        UN3 = 0b01100000,
        CON = 0b10000000,
        PDM = 0b10100000,
        UN6 = 0b11000000,
        POD = 0b11100000
    }


    public class RadioPacket
    {
        public uint address;
        public RadioPacketType type;
        public int sequence;
        public byte[] body;
        public byte rssi;

        public RadioPacket(uint address, RadioPacketType type, int sequence, byte[] body)
        {
            this.address = address;
            this.type = type;
            this.sequence = sequence % 32;
            this.body = body;
        }

        public static RadioPacket parse(byte[] data)
        {
            if (data.Length < 5)
                return null;

            var crc = data[data.Length - 1];
            var crc_computed = CrcUtil.Crc8(data.Sub(0, data.Length - 1));
            if (crc != crc_computed)
                return null;

            var address = data.GetUInt32(0);
            var type = (RadioPacketType)(data[4] & 0b11100000);
            var sequence = data[4] & 0b00011111;
            var body = data.Sub(5);
            return new RadioPacket(address, type, sequence, body);
        }

        public RadioPacket with_sequence(int sequence)
        {
            this.sequence = sequence;
            return this;
        }

        public byte[] get_data()
        {
            var data = this.address.ToBytes();
            data.Append((byte)((int)this.type | this.sequence));
            data.Append(this.body);
            data.Append(CrcUtil.Crc8(data));
            return data;
        }

        public override string ToString()
        {
            if (this.type == RadioPacketType.CON)
            {
                return $"{this.sequence:%02x} {this.type.ToString().Substring(0, 3)} {this.address:%08x} {this.body.Hex()}";
            }
            else
            {
                return $"{this.sequence:%02x} {this.type.ToString().Substring(0, 3)} {this.address:%08x} {this.body.Sub(0, 4).Hex()} {this.body.Sub(4).Hex()}";
            }
        }
    }

    public class BaseMessage
    {
        public uint? address = null;
        public int? sequence = null;
        public bool expect_critical_followup = false;
        public int body_length = 0;
        public byte[] body = null;
        public byte[] body_prefix = null;
        public List<Tuple<byte, byte[],uint?>> parts = new List<Tuple<byte, byte[],uint?>>();
        public string message_str_prefix = "\n";
        public RadioPacketType? type = null;

        public bool add_radio_packet(RadioPacket radio_packet)
        {
            if (radio_packet.type == RadioPacketType.POD || radio_packet.type == RadioPacketType.PDM)
            {
                this.type = radio_packet.type;
                this.address = radio_packet.body.GetUInt32(0);

                this.sequence = (radio_packet.body[4] >> 2) & 0x0f;
                this.expect_critical_followup = (radio_packet.body[4] & 0x80) > 0;
                this.body_length = ((radio_packet.body[4] & 0x03) << 8) | radio_packet.body[5];
                this.body_prefix = radio_packet.body.Sub(0, 6);
                this.body = radio_packet.body.Sub(6);
            }
            else
            {
                if (radio_packet.type == RadioPacketType.CON)
                    this.body.Append(radio_packet.body);
                else
                    throw new ProtocolError("Packet type invalid");
            }

            if (this.body_length == this.body.Length - 2)
            {
                var crc = this.body.GetUInt16(this.body.Length - 2);
                var crc_calculated = CrcUtil.Crc16(this.body_prefix.Append(this.body.Sub(0, this.body.Length - 2)));

                if (crc == crc_calculated)
                {
                    this.body = this.body.Sub(0, this.body.Length - 2);
                    var bi = 0;
                    while (bi < this.body.Length)
                    {
                        var response_type = this.body[bi];
                        var response_len = 0;
                        if (response_type == 0x1d)
                        {
                            response_len = this.body.Length - bi - 1;
                            bi += 1;
                        }
                        else
                        {
                            response_len = this.body[bi + 1];
                            bi += 2;
                        }

                        if (bi + response_len > this.body.Length)
                            throw new ProtocolError("Error in message format");

                        var response_body = this.body.Sub(bi, bi + response_len);
                        this.parts.Add(new Tuple<byte, byte[], uint?>(response_type, response_body, null));
                        bi += response_len;
                    }
                    return true;
                }
                else
                {
                    throw new ProtocolError("Message crc error");
                }
            }
            else
            {
                return false;
            }
        }

        public List<Tuple<byte, byte[], uint?>> get_parts()
        {
            return this.parts;
        }

        public List<RadioPacket> get_radio_packets(uint message_address,
                            int message_sequence,
                            uint packet_address,
                            int first_packet_sequence,
                            bool expect_critical_follow_up=false,
                            bool double_take=false)
        {
            this.message_str_prefix = $"{message_address:%08X} {message_sequence:%02X} {expect_critical_followup} ";

            this.sequence = message_sequence;
            this.expect_critical_followup = expect_critical_follow_up;
            this.address = message_address;

            var message_body_len = 0;
            foreach(var p in this.parts)
            {
                var cmd_body = p.Item2;
                var nonce = p.Item3;
                message_body_len += cmd_body.Length + 2;
                if (nonce != null)
                    message_body_len += 4;
            }

            byte b0 = 0;
            if (expect_critical_follow_up)
                b0 = 0x80;

            b0 |= (byte)(message_sequence << 2);
            b0 |= (byte)((message_body_len >> 8) & 0x03);
            byte b1 = (byte)(message_body_len & 0xff);

            var message_body = message_address.ToBytes();
            message_body.Append(b0);
            message_body.Append(b1);

            foreach(var p in this.parts)
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
                    message_body.Append(nonce.Value.ToBytes());
                }
                message_body.Append(cmd_body);
            }
            var crc_calculated = CrcUtil.Crc16(message_body);
            message_body.Append(crc_calculated.ToBytes());

            int index = 0;
            bool first_packet = true;
            int sequence = first_packet_sequence;
            int total_body_len = message_body.Length;
            var radio_packets = new List<RadioPacket>();

            while(index < total_body_len)
            {
                var to_write = Math.Min(31, total_body_len - index);
                var packet_body = message_body.Sub(index, index + to_write);
                            radio_packets.Add(new RadioPacket(packet_address,
                                                             first_packet ? this.type.Value : RadioPacketType.CON,
                                                             sequence,
                                                             packet_body));
                first_packet = false;
                index += to_write;
                sequence = (sequence + 2) % 32;
            }

            if (double_take)
            {
                var fp = radio_packets[0];
                radio_packets.Insert(0, fp);
            }
            return radio_packets;
        }

        public void add_part(PdmRequest cmd_type, byte[] cmd_body)
        {
            this.parts.Add(new Tuple<byte, byte[], uint?>((byte)cmd_type, cmd_body, null));
        }
    }

    public class PodMessage : BaseMessage
    {
        public PodMessage():base()
        {
            base.type = RadioPacketType.POD;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{this.address:%08X} {this.sequence:%02X} {this.expect_critical_followup} ");
            foreach(var p in this.parts)
            {
                sb.Append($"{p.Item1:%02X} {p.Item2.Hex()} ");
            }
            return sb.ToString();
        }
    }

    public class PdmMessage : BaseMessage
    {
        public TxPower? TxLevel;
        public uint? AckAddressOverride;
        public bool DoubleTake;

        public PdmMessage(PdmRequest cmd_type, byte[] cmd_body):base()
        {
            this.add_part(cmd_type, cmd_body);
            this.message_str_prefix = "\n";
            this.type = RadioPacketType.PDM;
        }

        public void set_nonce(uint nonce)
        {
            var part = this.parts[0];
            this.parts[0] = new Tuple<byte, byte[], uint?>(part.Item1, part.Item2, nonce);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.message_str_prefix);
            foreach (var p in this.parts)
            {
                if (p.Item3 == null)
                    sb.Append($"{p.Item1:%02X} {p.Item2.Hex()} ");
                else
                    sb.Append($"{p.Item1:%02X} {p.Item3.Value:%08X} {p.Item2.Hex()} ");
            }
            return sb.ToString();
        }
    }
}


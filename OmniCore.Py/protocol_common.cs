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
        public Bytes body;
        public byte rssi;

        public RadioPacket(uint address, RadioPacketType type, int sequence, Bytes body)
        {
            this.address = address;
            this.type = type;
            this.sequence = sequence % 32;
            this.body = body;
        }

        public static RadioPacket parse(Bytes data)
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
            return new RadioPacket(address, type, sequence, body);
        }

        public RadioPacket with_sequence(int sequence)
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

    public class BaseMessage
    {
        public uint? address = null;
        public int? sequence = null;
        public bool expect_critical_followup = false;
        public int body_length = 0;
        public Bytes body = null;
        public Bytes body_prefix = null;
        public List<Tuple<byte, Bytes, uint?>> parts = new List<Tuple<byte, Bytes, uint?>>();
        public string message_str_prefix = "\n";
        public RadioPacketType? type = null;
        public TxPower? TxLevel;
        public uint? AckAddressOverride;
        public bool DoubleTake;

        public bool add_radio_packet(RadioPacket radio_packet)
        {
            if (radio_packet.type == RadioPacketType.POD || radio_packet.type == RadioPacketType.PDM)
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
                if (radio_packet.type == RadioPacketType.CON)
                    this.body.Append(radio_packet.body);
                else
                    throw new ProtocolError("Packet type invalid");
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
                            var response_len = this.body[bi+1];
                            response_body = this.body.Sub(bi + 2, bi + 2 + response_len);
                            bi += response_len + 2;
                        }
                        this.parts.Add(new Tuple<byte, Bytes, uint?>(response_type, response_body, null));
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

        public List<Tuple<byte, Bytes, uint?>> get_parts()
        {
            return this.parts;
        }

        public List<RadioPacket> get_radio_packets(int first_packet_sequence)
        {
            this.message_str_prefix = $"{this.address:%08X} {this.sequence:%02X} {this.expect_critical_followup} ";

            var message_body_len = 0;
            foreach(var p in this.parts)
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
            var radio_packets = new List<RadioPacket>();

            while(index < message_body.Length)
            {
                var to_write = Math.Min(31, message_body.Length - index);
                var packet_body = message_body.Sub(index, index + to_write);
                radio_packets.Add(new RadioPacket(AckAddressOverride.Value,
                                                first_packet ? this.type.Value : RadioPacketType.CON,
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
                sb.Append($"{p.Item1:%02X} {p.Item2.ToHex()} ");
            }
            return sb.ToString();
        }
    }

    public class PdmMessage : BaseMessage
    {
        public PdmMessage(PdmRequest cmd_type, Bytes cmd_body):base()
        {
            this.add_part(cmd_type, cmd_body);
            this.message_str_prefix = "\n";
            this.type = RadioPacketType.PDM;
        }

        public void set_nonce(uint nonce)
        {
            var part = this.parts[0];
            this.parts[0] = new Tuple<byte, Bytes, uint?>(part.Item1, part.Item2, nonce);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.message_str_prefix);
            foreach (var p in this.parts)
            {
                if (p.Item3 == null)
                    sb.Append($"{p.Item1:%02X} {p.Item2.ToHex()} ");
                else
                    sb.Append($"{p.Item1:%02X} {p.Item3.Value:%08X} {p.Item2.ToHex()} ");
            }
            return sb.ToString();
        }
    }
}


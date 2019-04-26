using OmniCore.py;
using System;
using System.Collections.Generic;

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
        uint? address = null;
        int? sequence = null;
        bool expect_critical_followup = false;
        int body_length = 0;
        byte[] body = null;
        byte[] body_prefix = null;
        List<Tuple<byte, byte[],uint?>> parts = new List<Tuple<byte, byte[],uint?>>();
        string message_str_prefix = "\n";
        RadioPacketType? type = null;

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

        public void add_part(byte cmd_type, byte[] cmd_body)
        {
            this.parts.Add(new Tuple<byte, byte[], uint?>(cmd_type, cmd_body, null));
        }
    }
}




//class PodMessage(BaseMessage):
//    def __init__(self):
//        super(PodMessage, self).__init__()
//        self.type = RadioPacketType.POD

//    def __str__(self):
//        s = "%08X %02X %s " % ( self.address,self.sequence,self.expect_critical_followup)

//        for r_type, r_body in self.parts:
//            s += "%02x %s " % (r_type, r_body.hex())
//        return s


//class PdmMessage(BaseMessage):
//    def __init__(self, cmd_type, cmd_body):
//        super(PdmMessage, self).__init__()
//        self.add_part(cmd_type, cmd_body)
//        self.message_str_prefix = "\n"
//        self.type = RadioPacketType.PDM


//    def set_nonce(self, nonce):
//        cmd_type, cmd_body, _ = self.parts[0]
//        self.parts[0] = cmd_type, cmd_body, nonce

//    def __str__(self):
//        s = self.message_str_prefix
//        for cmd_type, cmd_body, nonce in self.parts:
//            if nonce is None:
//                s += "%02x %s " % (cmd_type, cmd_body.hex())
//            else:
//                s += "%02x %08x %s " % (cmd_type, nonce, cmd_body.hex())
//        return s



//def getInsulinScheduleTableFromPulses(pulses):
//    iseTable = []
//    ptr = 0
//    while ptr < len(pulses):
//        if ptr == len(pulses) - 1:
//            iseTable.append(getIse(pulses[ptr], 0, False))
//            break

//        alternatingTable = pulses[ptr:]
//        for k in range(1, len(alternatingTable), 2):
//            alternatingTable[k] -= 1

//        pulse = alternatingTable[0]
//        others = alternatingTable[1:]
//        repeats = getRepeatCount(pulse, others)
//        if repeats > 15:
//            repeats = 15
//        if repeats > 0:
//            iseTable.append(getIse(pulse, repeats, True))
//        else:
//            pulse = pulses[ptr]
//            others = pulses[ptr + 1:]
//            repeats = getRepeatCount(pulse, others)
//            if repeats > 15:
//                repeats = 15
//            iseTable.append(getIse(pulse, repeats, False))
//        ptr += repeats + 1
//    return iseTable

//def getPulsesForHalfHours(halfHourUnits):
//    halfHourlyDeliverySubtotals = []
//    totalToDeliver = Decimal(0)
//    for hhunit in halfHourUnits:
//        totalToDeliver += hhunit
//        halfHourlyDeliverySubtotals.append(totalToDeliver)

//    pulses = []
//    totalDelivered = Decimal(0)
//    for subtotal in halfHourlyDeliverySubtotals:
//        toDeliver = subtotal - totalDelivered
//        pulseCount = int(toDeliver * Decimal(20))
//        totalDelivered += Decimal(pulseCount) / Decimal(20)
//        pulses.append(pulseCount)

//    return pulses




//def getIse(pulses, repeat, alternate):
//    ise = pulses & 0x03ff
//    ise |= repeat << 12
//    if alternate:
//        ise |= 0x0800
//    return ise

//def getRepeatCount(pulse, otherPulses):
//    repeatCount = 0
//    for other in otherPulses:
//        if pulse != other:
//            break
//        repeatCount += 1
//    return repeatCount


//def getStringBodyFromTable(table):
//    st = bytes()
//    for val in table:
//        st += struct.pack(">H", val)
//    return st


//def getChecksum(body):
//    checksum = 0
//    for c in body:
//        checksum += c
//    return checksum


//def getHalfHourPulseInterval(pulseCount):
//    if pulseCount == 0:
//        return 180000000
//    else:
//        return int(180000000 / pulseCount)


//def getPulseIntervalEntries(halfHourUnits):
//    list1 = []
//    index = 0
//    for hhu in halfHourUnits:
//        pulses10 = hhu * Decimal("200")
//        interval = 1800000000
//        if hhu > 0:
//            interval = int(Decimal("9000000") / hhu)

//        if interval < 200000:
//            raise PdmError()
//        elif interval > 1800000000:
//            raise PdmError()

//        list1.append((int(pulses10), int(interval), index))
//        index += 1

//    list2 = []
//    lastPulseInterval = None
//    subTotalPulses = 0
//    hh_indices = []

//    for pulses, interval, index in list1:
//        if lastPulseInterval is None:
//            subTotalPulses = pulses
//            lastPulseInterval = interval
//            hh_indices.append(index)
//        elif lastPulseInterval == interval:
//            if subTotalPulses + pulses < 65536 and subTotalPulses > 0:
//                subTotalPulses += pulses
//                hh_indices.append(index)
//            else:
//                list2.append((subTotalPulses, lastPulseInterval, hh_indices))
//                subTotalPulses = pulses
//                hh_indices = [index]
//        else:
//            list2.append((subTotalPulses, lastPulseInterval, hh_indices))
//            subTotalPulses = pulses
//            lastPulseInterval = interval
//            hh_indices = [index]
//    else:
//        if lastPulseInterval >= 0:
//            list2.append((subTotalPulses, lastPulseInterval, hh_indices))

//    return list2

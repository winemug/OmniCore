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

}


class RadioPacket:
    def __init__(self, address, type, sequence, body):
        self.address = address
        self.type = type
        self.sequence = sequence % 32
        self.body = body

    @staticmethod
    def parse(data):
        if len(data) < 5:
            #raise ProtocolError("Packet length too small")
            return None

        crc = data[-1]
        crc_computed = crc8(data[:-1])
        if crc != crc_computed:
            #raise ProtocolError("Packet crc error")
            return None

        address = struct.unpack(">I", data[0:4])[0]

        type = RadioPacketType(data[4] & 0b11100000)
        sequence = data[4] & 0b00011111

        body = data[5:-1]
        return RadioPacket(address, type, sequence, body)

    def with_sequence(self, sequence):
        self.sequence = sequence
        return self

    def get_data(self):
        data = struct.pack(">I", self.address)
        data += bytes([self.type | self.sequence ])
        data += self.body
        data +=  bytes([crc8(data)])
        return data

    def __str__(self):
            #return "Packet Addr: 0x%08x Type: %s Seq: 0x%02x Body: %s" % (self.address, self.type, self.sequence, self.body.hex())
            if self.type == RadioPacketType.CON:
                return "%02x %s %08x %s" % (self.sequence, str(self.type)[-3:], self.address, self.body.hex())
            else:
                return "%02x %s %08x %s %s" % (self.sequence, str(self.type)[-3:], self.address,
                                                       self.body[0:4].hex(),
                                                       self.body[4:].hex())

class BaseMessage:
    def __init__(self):
        self.address = None
        self.sequence = None
        self.expect_critical_followup = False
        self.body_length = 0
        self.body = None
        self.body_prefix = None
        self.parts = []
        self.message_str_prefix = "\n"
        self.type = None

    def add_radio_packet(self, radio_packet):
        if radio_packet.type == RadioPacketType.POD or radio_packet.type == RadioPacketType.PDM:
            self.type = radio_packet.type
            self.address = struct.unpack(">I", radio_packet.body[0:4])[0]
            self.sequence = (radio_packet.body[4] >> 2) & 0x0f
            self.expect_critical_followup = (radio_packet.body[4] & 0x80) > 0
            self.body_length = ((radio_packet.body[4] & 0x03) << 8) | radio_packet.body[5]
            self.body_prefix = radio_packet.body[:6]
            self.body = radio_packet.body[6:]
        elif radio_packet.type == RadioPacketType.CON:
            self.body += radio_packet.body
        else:
            raise ProtocolError("Packet type invalid")

        if self.body_length == len(self.body) - 2:
            crc = struct.unpack(">H", self.body[-2:])[0]
            crc_calculated = crc16(self.body_prefix + self.body[:-2])
            if crc == crc_calculated:
                self.body = self.body[:-2]

                bi = 0
                while bi < len(self.body):
                    response_type = self.body[bi]
                    if response_type == 0x1d:
                        response_len = len(self.body) - bi - 1
                        bi += 1
                    else:
                        response_len = self.body[bi+1]
                        bi += 2

                    if bi+response_len > len(self.body):
                        raise ProtocolError("Error in message format")

                    response_body = self.body[bi:bi+response_len]
                    self.parts.append((response_type, response_body))
                    bi += response_len
                return True
            else:
                raise ProtocolError("Message crc error")
        else:
            return False

    def get_parts(self):
        return self.parts

    def get_radio_packets(self, message_address,
                    message_sequence,
                    packet_address,
                    first_packet_sequence,
                    expect_critical_follow_up=False,
                    double_take=False):

        self.message_str_prefix = "%08X %02X %s " % (
                                    message_address, message_sequence, expect_critical_follow_up)

        self.sequence = message_sequence
        self.expect_critical_followup = expect_critical_follow_up
        self.address = message_address

        message_body_len = 0
        for _, cmd_body, nonce in self.parts:
            message_body_len += len(cmd_body) + 2
            if nonce is not None:
                message_body_len += 4

        if expect_critical_follow_up:
            b0 = 0x80
        else:
            b0 = 0x00

        b0 |= (message_sequence << 2)
        b0 |= (message_body_len >> 8) & 0x03
        b1 = message_body_len & 0xff

        message_body = struct.pack(">I", message_address)
        message_body +=  bytes([b0, b1])
        for cmd_type, cmd_body, nonce in self.parts:
            if nonce is None:
                if cmd_type == PodResponse.Status:
                    message_body += bytes([cmd_type])
                else:
                    message_body += bytes([cmd_type, len(cmd_body)])
            else:
                message_body += bytes([cmd_type, len(cmd_body) + 4])
                message_body += struct.pack(">I", nonce)
            message_body += cmd_body

        crc_calculated = crc16(message_body)
        x = struct.pack(">H", crc_calculated)
        message_body += x

        index = 0
        first_packet = True
        sequence = first_packet_sequence
        total_body_len = len(message_body)
        radio_packets = []
        while(index < total_body_len):
            to_write = min(31, total_body_len - index)
            packet_body = message_body[index:index+to_write]

            radio_packets.append(RadioPacket(packet_address,
                                             self.type if first_packet else RadioPacketType.CON,
                                             sequence,
                                             packet_body))
            first_packet = False
            index += to_write
            sequence = (sequence + 2) % 32

        if double_take:
            return [radio_packets[0]] + radio_packets
        else:
            return radio_packets

    def add_part(self, cmd_type, cmd_body):
        part_tuple = cmd_type, cmd_body, None
        self.parts.append(part_tuple)


class PodMessage(BaseMessage):
    def __init__(self):
        super(PodMessage, self).__init__()
        self.type = RadioPacketType.POD

    def __str__(self):
        s = "%08X %02X %s " % ( self.address,self.sequence,self.expect_critical_followup)

        for r_type, r_body in self.parts:
            s += "%02x %s " % (r_type, r_body.hex())
        return s


class PdmMessage(BaseMessage):
    def __init__(self, cmd_type, cmd_body):
        super(PdmMessage, self).__init__()
        self.add_part(cmd_type, cmd_body)
        self.message_str_prefix = "\n"
        self.type = RadioPacketType.PDM


    def set_nonce(self, nonce):
        cmd_type, cmd_body, _ = self.parts[0]
        self.parts[0] = cmd_type, cmd_body, nonce

    def __str__(self):
        s = self.message_str_prefix
        for cmd_type, cmd_body, nonce in self.parts:
            if nonce is None:
                s += "%02x %s " % (cmd_type, cmd_body.hex())
            else:
                s += "%02x %08x %s " % (cmd_type, nonce, cmd_body.hex())
        return s



def getInsulinScheduleTableFromPulses(pulses):
    iseTable = []
    ptr = 0
    while ptr < len(pulses):
        if ptr == len(pulses) - 1:
            iseTable.append(getIse(pulses[ptr], 0, False))
            break

        alternatingTable = pulses[ptr:]
        for k in range(1, len(alternatingTable), 2):
            alternatingTable[k] -= 1

        pulse = alternatingTable[0]
        others = alternatingTable[1:]
        repeats = getRepeatCount(pulse, others)
        if repeats > 15:
            repeats = 15
        if repeats > 0:
            iseTable.append(getIse(pulse, repeats, True))
        else:
            pulse = pulses[ptr]
            others = pulses[ptr + 1:]
            repeats = getRepeatCount(pulse, others)
            if repeats > 15:
                repeats = 15
            iseTable.append(getIse(pulse, repeats, False))
        ptr += repeats + 1
    return iseTable

def getPulsesForHalfHours(halfHourUnits):
    halfHourlyDeliverySubtotals = []
    totalToDeliver = Decimal(0)
    for hhunit in halfHourUnits:
        totalToDeliver += hhunit
        halfHourlyDeliverySubtotals.append(totalToDeliver)

    pulses = []
    totalDelivered = Decimal(0)
    for subtotal in halfHourlyDeliverySubtotals:
        toDeliver = subtotal - totalDelivered
        pulseCount = int(toDeliver * Decimal(20))
        totalDelivered += Decimal(pulseCount) / Decimal(20)
        pulses.append(pulseCount)

    return pulses




def getIse(pulses, repeat, alternate):
    ise = pulses & 0x03ff
    ise |= repeat << 12
    if alternate:
        ise |= 0x0800
    return ise

def getRepeatCount(pulse, otherPulses):
    repeatCount = 0
    for other in otherPulses:
        if pulse != other:
            break
        repeatCount += 1
    return repeatCount


def getStringBodyFromTable(table):
    st = bytes()
    for val in table:
        st += struct.pack(">H", val)
    return st


def getChecksum(body):
    checksum = 0
    for c in body:
        checksum += c
    return checksum


def getHalfHourPulseInterval(pulseCount):
    if pulseCount == 0:
        return 180000000
    else:
        return int(180000000 / pulseCount)


def getPulseIntervalEntries(halfHourUnits):
    list1 = []
    index = 0
    for hhu in halfHourUnits:
        pulses10 = hhu * Decimal("200")
        interval = 1800000000
        if hhu > 0:
            interval = int(Decimal("9000000") / hhu)

        if interval < 200000:
            raise PdmError()
        elif interval > 1800000000:
            raise PdmError()

        list1.append((int(pulses10), int(interval), index))
        index += 1

    list2 = []
    lastPulseInterval = None
    subTotalPulses = 0
    hh_indices = []

    for pulses, interval, index in list1:
        if lastPulseInterval is None:
            subTotalPulses = pulses
            lastPulseInterval = interval
            hh_indices.append(index)
        elif lastPulseInterval == interval:
            if subTotalPulses + pulses < 65536 and subTotalPulses > 0:
                subTotalPulses += pulses
                hh_indices.append(index)
            else:
                list2.append((subTotalPulses, lastPulseInterval, hh_indices))
                subTotalPulses = pulses
                hh_indices = [index]
        else:
            list2.append((subTotalPulses, lastPulseInterval, hh_indices))
            subTotalPulses = pulses
            lastPulseInterval = interval
            hh_indices = [index]
    else:
        if lastPulseInterval >= 0:
            list2.append((subTotalPulses, lastPulseInterval, hh_indices))

    return list2
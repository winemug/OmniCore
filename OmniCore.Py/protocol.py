from podcomm.protocol_common import *
from podcomm.definitions import *
from enum import IntEnum
from decimal import Decimal
import struct
import time


DECIMAL_0_05 = Decimal("0.05")
DECIMAL_2_00 = Decimal("2")

class StatusRequestType(IntEnum):
    Standard = 0


def request_assign_address(address):
    cmd_body = struct.pack(">I", address)
    return PdmMessage(PdmRequest.AssignAddress, cmd_body)


def request_setup_pod(lot, tid, address, year, month, day, hour, minute):
    cmd_body = struct.pack(">I", address)
    cmd_body += bytes([0x14, 0x04])

    cmd_body += bytes([month, day, year - 2000, hour, minute])

    cmd_body += struct.pack(">I", lot)
    cmd_body += struct.pack(">I", tid)
    return PdmMessage(PdmRequest.SetupPod, cmd_body)


def request_alert_setup(alert_configurations):
    cmd_body = bytes()

    for ac in alert_configurations:
        if ac.alert_after_minutes is None and ac.alert_after_reservoir is None and ac.activate:
            raise PdmError("Either alert_after_minutes or alert_after_reservoir must be set")
        elif ac.alert_after_minutes is not None and ac.alert_after_reservoir is not None:
            raise PdmError("Only one of alert_after_minutes or alert_after_reservoir must be set")

        if ac.alert_duration > 0x1FF:
            raise PdmError("Alert duration in minutes cannot be more than %d" % 0x1ff)
        elif ac.alert_duration < 0:
            raise PdmError("Invalid alert duration value")

        if ac.alert_after_minutes is not None and ac.alert_after_minutes > 4800:
            raise PdmError("Alert cannot be set beyond 80 hours")
        if ac.alert_after_minutes is not None and ac.alert_after_minutes < 0:
            raise PdmError("Invalid value for alert_after_minutes")

        if ac.alert_after_reservoir is not None and ac.alert_after_reservoir > 50:
            raise PdmError("Alert cannot be set for more than 50 units")
        if ac.alert_after_reservoir is not None and ac.alert_after_reservoir < 0:
            raise PdmError("Invalid value for alert_after_reservoir")

        b0 = ac.alert_index << 4
        if ac.activate:
            b0 |= 0x08
        if ac.alert_after_reservoir is not None:
            b0 |= 0x04
        if ac.trigger_auto_off:
            b0 |= 0x02

        b0 |= (ac.alert_duration >> 8) & 0x0001
        b1 = ac.alert_duration & 0x00ff
        b2 = 0
        b3 = 0
        if ac.alert_after_reservoir is not None:
            reservoir_limit = int(ac.alert_after_reservoir * 10)
            b2 = reservoir_limit >> 8
            b3 = reservoir_limit & 0x00ff
        if ac.alert_after_minutes is not None:
            b2 = ac.alert_after_minutes >> 8
            b3 = ac.alert_after_minutes & 0x00ff

        cmd_body += bytes([b0, b1, b2, b3, ac.beep_repeat_type, ac.beep_type])

    return PdmMessage(PdmRequest.ConfigureAlerts, cmd_body)


def request_set_basal_schedule(schedule, hour, minute, second):
    halved_schedule = []

    for entry in schedule:
        halved_schedule.append(entry / DECIMAL_2_00)

    current_hh = hour * 2
    if minute < 30:
        seconds_past_hh = minute * 60
    else:
        seconds_past_hh = (minute - 30) * 60
        current_hh += 1

    seconds_past_hh += second
    seconds_to_hh = 1800 - seconds_past_hh

    pulse_list = getPulsesForHalfHours(halved_schedule)
    ise_list = getInsulinScheduleTableFromPulses(pulse_list)
    ise_body = getStringBodyFromTable(ise_list)
    pulse_body = getStringBodyFromTable(pulse_list)

    command_body = bytes([0])

    body_checksum = bytes([current_hh])

    current_hh_pulse_count = pulse_list[current_hh]
    remaining_pulse_count = int(current_hh_pulse_count * seconds_to_hh / 1800)

    body_checksum += struct.pack(">H", seconds_to_hh * 8)
    body_checksum += struct.pack(">H", remaining_pulse_count)

    checksum = getChecksum(body_checksum + pulse_body)

    command_body += struct.pack(">H", checksum)
    command_body += body_checksum
    command_body += ise_body

    msg = PdmMessage(PdmRequest.InsulinSchedule, command_body)

    reminders = 0
    # if confidenceReminder:
    #     reminders |= 0x40

    command_body = bytes([reminders])

    pulse_entries = getPulseIntervalEntries(halved_schedule)
    table_index = 0
    for pulses10, interval, indices in pulse_entries:
        if current_hh in indices:
            command_body += bytes([table_index])
            ii = indices.index(current_hh)

            pulses_past_intervals = int(ii * 1800000000 / interval)
            pulses_past_this_interval = int(seconds_past_hh * 1000000 / interval) + 1
            remaining_pulses_this_interval = pulses10 - pulses_past_this_interval - pulses_past_intervals
            microseconds_to_next_interval = interval - (seconds_past_hh * 1000000 % interval)

            command_body += struct.pack(">H", remaining_pulses_this_interval)
            command_body += struct.pack(">I", microseconds_to_next_interval)
            break
        else:
            table_index += 1

    for pulse_count, interval, _ in pulse_entries:
        command_body += struct.pack(">H", pulse_count)
        command_body += struct.pack(">I", interval)

    msg.add_part(PdmRequest.BasalSchedule, command_body)
    return msg

def request_prime_cannula():
    return _bolus_message(52, pulse_speed=8, delivery_delay=1)


def request_insert_cannula():
    return _bolus_message(10, pulse_speed=8, delivery_delay=1)


def request_status(status_request_type=0):
    cmd_body = bytes([status_request_type])
    return PdmMessage(PdmRequest.Status, cmd_body)


def request_acknowledge_alerts(alert_mask):
    return PdmMessage(PdmRequest.AcknowledgeAlerts, bytes([alert_mask]))


def request_purge_insulin(iu_to_purge):
    return _bolus_message(pulse_count=int(iu_to_purge / DECIMAL_0_05),
                          pulse_speed=8,
                          delivery_delay=1)


def request_bolus(iu_bolus):
    return _bolus_message(pulse_count=int(iu_bolus / DECIMAL_0_05))


def request_cancel_bolus():
    return _cancel_activity_message(bolus=True)


def request_temp_basal(basal_rate_iuhr, duration_hours):
    half_hour_count = int(duration_hours * DECIMAL_2_00)
    hh_units = [basal_rate_iuhr / DECIMAL_2_00] * half_hour_count
    pulseList = getPulsesForHalfHours(hh_units)
    iseList = getInsulinScheduleTableFromPulses(pulseList)

    iseBody = getStringBodyFromTable(iseList)
    pulseBody = getStringBodyFromTable(pulseList)

    cmd_body = bytes([0x01])

    body_checksum = bytes([half_hour_count])
    body_checksum += struct.pack(">H", 0x3840)
    body_checksum += struct.pack(">H", pulseList[0])
    checksum = getChecksum(body_checksum + pulseBody)

    cmd_body += struct.pack(">H", checksum)
    cmd_body += body_checksum
    cmd_body += iseBody

    msg = PdmMessage(PdmRequest.InsulinSchedule, cmd_body)

    reminders = 0
    # if confidenceReminder:
    #     reminders |= 0x40

    cmd_body = bytes([reminders, 0x00])
    pulseEntries = getPulseIntervalEntries(hh_units)

    firstPulseCount, firstInterval, _ = pulseEntries[0]
    cmd_body += struct.pack(">H", firstPulseCount)
    cmd_body += struct.pack(">I", firstInterval)

    for pulseCount, interval, _ in pulseEntries:
        cmd_body += struct.pack(">H", pulseCount)
        cmd_body += struct.pack(">I", interval)

    msg.add_part(PdmRequest.TempBasalSchedule, cmd_body)
    return msg


def request_cancel_temp_basal():
    return _cancel_activity_message(temp_basal=True)


def request_stop_basal_insulin():
    return _cancel_activity_message(basal=True)


def request_deactivate():
    return PdmMessage(PdmRequest.DeactivatePod, bytes())


def request_delivery_flags(byte16, byte17):
    cmd_body = bytes([byte16, byte17])
    return PdmMessage(PdmRequest.SetDeliveryFlags, cmd_body)


def response_parse(response, pod):
    pod.nonce_syncword = None
    parts = response.get_parts()
    for response_type, response_body in parts:
        if response_type == PodResponse.VersionInfo:
            parse_version_response(response_body, pod)
        elif response_type == PodResponse.DetailInfo:
            parse_information_response(response_body, pod)
        elif response_type == PodResponse.ResyncRequest:
            parse_resync_response(response_body, pod)
        elif response_type == PodResponse.Status:
            parse_status_response(response_body, pod)
        else:
            raise ProtocolError("Unknown response type %02X" % response_type)


def parse_information_response(response, pod):
        if response[0] == 0x01:
            pod.state_alerts = struct.unpack(">8H", response[3:])
        elif response[0] == 0x02:
            pod.state_last_updated = time.time()
            pod.state_faulted = True
            pod.state_progress = response[1]
            parse_delivery_state(pod, response[2])
            pod.insulin_canceled = struct.unpack(">H", response[3:5])[0] * 0.05
            pod.radio_message_sequence = response[5]
            pod.insulin_delivered = struct.unpack(">H", response[6:8])[0] * 0.05
            pod.fault_event = response[8]
            pod.fault_event_rel_time = struct.unpack(">H", response[9:11])[0]
            pod.insulin_reservoir = struct.unpack(">H", response[11:13])[0] * 0.05
            pod.state_active_minutes = struct.unpack(">H", response[13:15])[0]
            pod.state_alert = response[15]
            pod.fault_table_access = response[16]
            pod.fault_insulin_state_table_corruption = response[17] >> 7
            pod.fault_internal_variables = (response[17] & 0x60) >> 6
            pod.fault_immediate_bolus_in_progress = (response[17] & 0x10) >> 4
            pod.fault_progress_before = (response[17] & 0x0F)
            pod.radio_low_gain = (response[18] & 0xC0) >> 6
            pod.radio_rssi = response[18] & 0x3F
            pod.fault_progress_before_2 = (response[19] & 0x0F)
            pod.fault_information_type2_last_word = struct.unpack(">H", response[20:22])[0]
        elif response[0] == 0x03:
            pass
        elif response[0] == 0x05:
            pass
        elif response[0] == 0x06:
            pass
        elif response[0] == 0x46:
            pass
        elif response[0] == 0x50:
            pass
        elif response[0] == 0x51:
            pass
        else:
            raise ProtocolError("Failed to parse the information response of type 0x%2X with content: %s"
                                % (response[0], response.hex()))


def parse_resync_response(response, pod):
    if response[0] == 0x14:
        pod.nonce_syncword = struct.unpack(">H", response[1:])[0]
    else:
        raise ProtocolError("Unknown resync request 0x%2x from pod" % response[0])


def parse_status_response(response, pod):
    pod.state_last_updated = time.time()
    s = struct.unpack(">BII", response)

    parse_delivery_state(pod, s[0] >> 4)
    pod.state_progress = PodProgress(s[0] & 0xF)

    pod.radio_message_sequence = (s[1] & 0x00007800) >> 11

    pod.insulin_delivered = ((s[1] & 0x0FFF8000) >> 15) * 0.05
    pod.insulin_canceled = (s[1] & 0x000007FF) * 0.05

    pod.state_faulted = ((s[2] >> 31) != 0)
    pod.state_alert = (s[2] >> 23) & 0xFF
    pod.state_active_minutes = (s[2] & 0x007FFC00) >> 10
    pod.insulin_reservoir = (s[2] & 0x000003FF) * 0.05


def parse_delivery_state(pod, delivery_state):
    if delivery_state & 8 > 0:
        pod.state_bolus = BolusState.Extended
    elif delivery_state & 4 > 0:
        pod.state_bolus = BolusState.Immediate
    else:
        pod.state_bolus = BolusState.NotRunning

    if delivery_state & 2 > 0:
        pod.state_basal = BasalState.TempBasal
    elif delivery_state & 1 > 0:
        pod.state_basal = BasalState.Program
    else:
        pod.state_basal = BasalState.NotRunning


def parse_version_response(response, pod):
    pod.state_last_updated = time.time()
    if len(response) == 27:
        pod.id_version_unknown_7_bytes = response[0:7].hex()
        response = response[7:]

    mx = response[0]
    my = response[1]
    mz = response[2]
    pod.id_version_pm = "%d.%d.%d" % (mx, my, mz)

    ix = response[3]
    iy = response[4]
    iz = response[5]
    pod.id_version_pi = "%d.%d.%d" % (ix, iy, iz)

    pod.id_version_unknown_byte = "%d" % response[6]
    pod.state_progress = response[7] & 0x0F
    pod.id_lot = struct.unpack(">I", response[8:12])[0]
    pod.id_t = struct.unpack(">I", response[12:16])[0]
    if len(response) == 21:
        pod.radio_low_gain = response[17] >> 6
        pod.radio_rssi = response[17] & 0b00111111
        pod.radio_address = struct.unpack(">I", response[17:21])[0]
    else:
        pod.radio_address = struct.unpack(">I", response[16:20])[0]


def _bolus_message(pulse_count, pulse_speed=16, reminders=0, delivery_delay=2):
    commandBody = bytes([0x02])

    bodyForChecksum = b"\x01"
    pulse_span = pulse_speed * pulse_count
    bodyForChecksum += struct.pack(">H", pulse_span)
    bodyForChecksum += struct.pack(">H", pulse_count)
    bodyForChecksum += struct.pack(">H", pulse_count)
    checksum = getChecksum(bodyForChecksum)

    commandBody += struct.pack(">H", checksum)
    commandBody += bodyForChecksum

    msg = PdmMessage(PdmRequest.InsulinSchedule, commandBody)

    commandBody = bytes([reminders])
    commandBody += struct.pack(">H", pulse_count * 10)
    commandBody += struct.pack(">I", delivery_delay * 100000)
    commandBody += b"\x00\x00\x00\x00\x00\x00"
    msg.add_part(PdmRequest.BolusSchedule, commandBody)

    return msg


def _cancel_activity_message(basal=False, bolus=False, temp_basal=False):
    c = 0

    if bolus:
        c = c | 0x04
    if temp_basal:
        c = c | 0x02
    if basal:
        c = c | 0x01
    cmd_body = bytes([c])

    msg = PdmMessage(PdmRequest.CancelDelivery, cmd_body)
    return msg


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

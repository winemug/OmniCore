using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Py
{
    public static class protocol
    {
        public static PdmMessage request_assign_address(uint address)
        {
            var cmd_body = address.ToBytes();
            return new PdmMessage(PdmRequest.AssignAddress, cmd_body);
        }

        public static PdmMessage request_setup_pod(int lot, int tid, uint address,
            int year, byte month, byte day, byte hour, byte minute)
        {
            var cmd_body = address.ToBytes();
            cmd_body.Append(new byte[] { 0x14, 0x04 });
            cmd_body.Append(new byte[] { month, day, (byte)(year - 2000), hour, minute });
            cmd_body.Append(lot.ToBytes());
            cmd_body.Append(tid.ToBytes());
            return new PdmMessage(PdmRequest.SetupPod, cmd_body);
        }

        public static PdmMessage request_alert_setup(List<AlertConfiguration> alert_configurations)
        {
            var cmd_body = new byte[0];
            foreach (var ac in alert_configurations)
            {
                if (ac.alert_after_minutes == null && ac.alert_after_reservoir == null && ac.activate)
                    throw new PdmError("Either alert_after_minutes or alert_after_reservoir must be set");
                else if (ac.alert_after_minutes != null && ac.alert_after_reservoir != null)
                    throw new PdmError("Only one of alert_after_minutes or alert_after_reservoir must be set");

                if (ac.alert_duration > 0x1FF)
                    throw new PdmError($"Alert duration in minutes cannot be more than {0x1ff:%d}");
                else if (ac.alert_duration < 0)
                    throw new PdmError("Invalid alert duration value");

                if (ac.alert_after_minutes != null && ac.alert_after_minutes > 4800)
                    throw new PdmError("Alert cannot be set beyond 80 hours");
                if (ac.alert_after_minutes != null && ac.alert_after_minutes < 0)
                    throw new PdmError("Invalid value for alert_after_minutes");

                if (ac.alert_after_reservoir != null && ac.alert_after_reservoir > 50)
                    throw new PdmError("Alert cannot be set for more than 50 units");
                if (ac.alert_after_reservoir != null && ac.alert_after_reservoir < 0)
                    throw new PdmError("Invalid value for alert_after_reservoir");

                byte b0 = (byte)(ac.alert_index << 4);
                if (ac.activate)
                    b0 |= 0x08;
                if (ac.alert_after_reservoir != null)
                    b0 |= 0x04;
                if (ac.trigger_auto_off)
                    b0 |= 0x02;

                b0 |= (byte)((ac.alert_duration >> 8) & 0x01);
                byte b1 = (byte)(ac.alert_duration & 0x00ff);
                byte b2 = 0;
                byte b3 = 0;

                if (ac.alert_after_reservoir != null)
                {
                    var reservoir_limit = (int)(ac.alert_after_reservoir * 10);
                    b2 = (byte)(reservoir_limit >> 8);
                    b3 = (byte)(reservoir_limit & 0xff);
                }
                if (ac.alert_after_minutes != null)
                {
                    b2 = (byte)(ac.alert_after_minutes >> 8);
                    b3 = (byte)(ac.alert_after_minutes & 0xff);
                }

                cmd_body.Append(new byte[] { b0, b1, b2, b3, (byte)ac.beep_repeat_type, (byte)ac.beep_type });
            }
            return new PdmMessage(PdmRequest.ConfigureAlerts, cmd_body);
        }

        public static PdmMessage request_status(int status_request_type= 0)
        {
            var cmd_body = new byte[] { (byte)status_request_type };
            return new PdmMessage(PdmRequest.Status, cmd_body);
        }

        public static PdmMessage request_acknowledge_alerts(byte alert_mask)
        {
            return new PdmMessage(PdmRequest.AcknowledgeAlerts, new byte[] { alert_mask });
        }

        public static PdmMessage request_deactivate()
        {
            return new PdmMessage(PdmRequest.DeactivatePod, new byte[0]);
        }

        public static PdmMessage request_delivery_flags(byte byte16, byte byte17)
        {
            return new PdmMessage(PdmRequest.SetDeliveryFlags, new byte[] { byte16, byte17 });
        }

        public static PdmMessage request_cancel_bolus()
        {
            return new PdmMessage(PdmRequest.CancelDelivery, new byte[] { 0x04 });
        }

        public static PdmMessage request_cancel_temp_basal()
        {
            return new PdmMessage(PdmRequest.CancelDelivery, new byte[] { 0x02 });
        }

        public static PdmMessage request_stop_basal_insulin()
        {
            return new PdmMessage(PdmRequest.CancelDelivery, new byte[] { 0x01 });
        }

        public static PdmMessage request_temp_basal(decimal basal_rate_iuhr, decimal duration_hours)
        {
            var half_hour_count = (int)(duration_hours * 2.0m);
            var hh_units = new decimal[half_hour_count];
            for (int i = 0; i < half_hour_count; i++)
                hh_units[i] = basal_rate_iuhr / 2.0m;

            var pulseList = getPulsesForHalfHours(hh_units);
            var iseList = getInsulinScheduleTableFromPulses(pulseList);

            var iseBody = getBodyFromTable(iseList);
            var pulseBody = getBodyFromTable(pulseList);

            var cmd_body = new byte[] { 0x01 };

            var body_checksum = new byte[] { (byte)half_hour_count };
            ushort b1 = 0x3840;
            body_checksum.Append(b1.ToBytes());
            body_checksum.Append(pulseList[0].ToBytes());
            var checksum = getChecksum(body_checksum.Append(pulseBody));

            cmd_body.Append(checksum.ToBytes());
            cmd_body.Append(body_checksum);
            cmd_body.Append(iseBody);

            var msg = new PdmMessage(PdmRequest.InsulinSchedule, cmd_body);

            byte reminders = 0;
            //#if confidenceReminder:
            //# reminders |= 0x40

            cmd_body = new byte[] { reminders, 0x00 };
            var pulseEntries = getPulseIntervalEntries(hh_units);

            var firstPte = pulseEntries[0];
            cmd_body.Append(firstPte.Item1.ToBytes());
            cmd_body.Append(firstPte.Item2.ToBytes());

            foreach(var pte in pulseEntries)
            {
                cmd_body.Append(pte.Item1.ToBytes());
                cmd_body.Append(pte.Item2.ToBytes());
            }

            msg.add_part(PdmRequest.TempBasalSchedule, cmd_body);
            return msg;
        }


        private static ushort[] getPulsesForHalfHours(decimal[] halfHourUnits)
        {
            var count = halfHourUnits.Length;
            var halfHourlyDeliverySubtotals = new decimal[count];
            var totalToDeliver = 0m;
            for (int i = 0; i < count; i++)
            {
                totalToDeliver += halfHourUnits[i];
                halfHourlyDeliverySubtotals[i] = totalToDeliver;
            }

            var pulses = new ushort[count];
            var totalDelivered = 0m;
            for (int i = 0; i < count; i++)
            {
                var subtotal = halfHourlyDeliverySubtotals[i];
                var toDeliver = subtotal - totalDelivered;
                var pulseCount = (int)(toDeliver * 20m);
                totalDelivered += (decimal)pulseCount / 20m;
                pulses[i] = (ushort)pulseCount;
            }
            return pulses;
        }

        private static ushort getIse(ushort pulses, ushort repeat, bool alternate)
        {
            var ise = (ushort)(pulses & 0x03ff);
            ise |= (ushort) (repeat << 12);
            if (alternate)
                ise |= 0x0800;
            return ise;
        }

        private static ushort getRepeatCount(ushort pulse, ushort[] others)
        {
            ushort repeatCount = 0;
            foreach(var other in others)
            {
                if (pulse != other)
                    break;
                repeatCount++;
            }
            return repeatCount;
        }

        private static ushort[] getInsulinScheduleTableFromPulses(ushort[] pulses)
        {
            var count = pulses.Length;
            var iseTable = new List<ushort>();
            int ptr = 0;
            while (ptr < count)
            {
                if (ptr == count - 1)
                {
                    iseTable.Add(getIse(pulses[ptr], 0, false));
                    break;
                }

                var alternatingTable = pulses.Sub(ptr);
                for (int k = 1; k < alternatingTable.Length; k += 2)
                    alternatingTable[k] -= 1;

                var pulse = alternatingTable[0];
                var others = alternatingTable.Sub(1);
                var repeats = getRepeatCount(pulse, others);

                if (repeats > 15)
                    repeats = 15;

                if (repeats > 0)
                {
                    iseTable.Add(getIse(pulse, repeats, true));
                }
                else
                {
                    pulse = pulses[ptr];
                    others = pulses.Sub(ptr + 1);
                    repeats = getRepeatCount(pulse, others);
                    if (repeats > 15)
                        repeats = 15;
                    iseTable.Add(getIse(pulse, repeats, false));
                }
                ptr += repeats + 1;
            }
            return iseTable.ToArray();
        }

        private static byte[] getBodyFromTable(ushort[] table)
        {
            byte[] body = new byte[table.Length * 2];
            for(int i=0; i<table.Length; i++)
            {
                var v = table[i];
                body[i*2] = (byte)(v >> 8);
                body[i * 2 + 1] = (byte)(v & 0xff);
            }
            return body;
        }

        private static ushort getChecksum(byte[] body)
        {
            ushort checksum = 0;
            foreach(byte b in body)
            {
                checksum += b;
            }
            return checksum;
        }

        private static int getHalfHourPulseInterval(uint pulseCount)
        {
            if (pulseCount == 0)
                return 180000000;
            else
                return (int)(180000000 / pulseCount);
        }

        private static Tuple<ushort,uint,int[]>[] getPulseIntervalEntries(decimal[] halfHourUnits)
        {
            var count = halfHourUnits.Length;
            var list1 = new Tuple<ushort,uint>[count];
            for(int i=0; i<count; i++)
            {
                var hhu = halfHourUnits[i];
                var p10 = (ushort)(hhu * 200m);
                uint interval = 1800000000;
                if (hhu > 0)
                    interval = (uint)(9000000m / hhu);
                if (interval < 200000)
                    throw new ArgumentException();
                if (interval > 1800000000)
                    throw new ArgumentException();

                list1[i] = new Tuple<ushort, uint>(p10, interval);
            }

            var list2 = new List<Tuple<ushort, uint, int[]>>();
            uint lastPulseInterval = 0xFFFFFFFF;
            ushort subTotalPulses = 0;
            var hh_indices = new List<int>();

            for(int i=0; i<count; i++)
            {
                var pulses = list1[i].Item1;
                var interval = list1[i].Item2;

                if (lastPulseInterval == 0xFFFFFFFF)
                {
                    subTotalPulses = pulses;
                    lastPulseInterval = interval;
                    hh_indices.Add(i);
                }
                else if (lastPulseInterval == interval)
                {
                    if (((int)subTotalPulses + (int)pulses) < 65536 && subTotalPulses > 0)
                    {
                        subTotalPulses += pulses;
                        hh_indices.Add(i);
                    }
                    else
                    {
                        list2.Add(new Tuple<ushort,uint,int[]>(subTotalPulses, lastPulseInterval, hh_indices.ToArray()));
                        subTotalPulses = pulses;
                        hh_indices = new List<int>(new int[] { i } );
                    }
                }
                else
                {
                    list2.Add(new Tuple<ushort, uint, int[]>(subTotalPulses, lastPulseInterval, hh_indices.ToArray()));
                    subTotalPulses = pulses;
                    lastPulseInterval = interval;
                    hh_indices = new List<int>(new int[] { i });
                }
            }

            if (lastPulseInterval >= 0)
            {
                list2.Add(new Tuple<ushort, uint, int[]>(subTotalPulses, lastPulseInterval, hh_indices.ToArray()));
            }

            return list2.ToArray();
        }
    }
}


        /*

def request_set_basal_schedule(schedule, hour, minute, second):
    halved_schedule = []

    for entry in schedule:
        halved_schedule.append(entry / DECIMAL_2_00)

    current_hh = hour* 2
    if minute< 30:
        seconds_past_hh = minute* 60
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

    body_checksum += struct.pack(">H", seconds_to_hh* 8)
    body_checksum += struct.pack(">H", remaining_pulse_count)

    checksum = getChecksum(body_checksum + pulse_body)

    command_body += struct.pack(">H", checksum)
    command_body += body_checksum
    command_body += ise_body

    msg = PdmMessage(PdmRequest.InsulinSchedule, command_body)

    reminders = 0
#if confidenceReminder:
# reminders |= 0x40

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


def request_purge_insulin(iu_to_purge):
    return _bolus_message(pulse_count=int(iu_to_purge / DECIMAL_0_05),
                          pulse_speed=8,
                          delivery_delay=1)


def request_bolus(iu_bolus):
    return _bolus_message(pulse_count=int(iu_bolus / DECIMAL_0_05))



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



*/
  
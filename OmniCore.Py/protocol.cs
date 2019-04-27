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

        public static PdmMessage request_status(int status_request_type = 0)
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

            foreach (var pte in pulseEntries)
            {
                cmd_body.Append(pte.Item1.ToBytes());
                cmd_body.Append(pte.Item2.ToBytes());
            }

            msg.add_part(PdmRequest.TempBasalSchedule, cmd_body);
            return msg;
        }

        public static PdmMessage request_prime_cannula()
        {
            return bolus_message(52, 8, 1);
        }

        public static PdmMessage request_insert_cannula()
        {
            return bolus_message(10, 8, 1);
        }

        public static PdmMessage request_bolus(decimal iu_bolus)
        {
            return bolus_message((ushort)(iu_bolus / 0.05m));
        }

        private static PdmMessage bolus_message(ushort pulse_count,
        int pulse_speed = 16, int delivery_delay = 2)
        {
            var commandBody = new byte[] { 0x02 };
            var bodyForChecksum = new byte[] { 0x01 };
            var pulse_span = (ushort)(pulse_speed * pulse_count);
            bodyForChecksum.Append(pulse_span.ToBytes());
            bodyForChecksum.Append(pulse_count.ToBytes());
            bodyForChecksum.Append(pulse_count.ToBytes());
            var checksum = getChecksum(bodyForChecksum);
            commandBody.Append(checksum.ToBytes());
            commandBody.Append(bodyForChecksum);

            var msg = new PdmMessage(PdmRequest.InsulinSchedule, commandBody);

            commandBody = new byte[] { 0x00 };
            ushort p10 = (ushort)(pulse_count * 10);
            commandBody.Append(p10.ToBytes());
            uint dd = (uint)delivery_delay * (uint)100000;
            commandBody.Append(dd.ToBytes());
            commandBody.Append(new byte[] { 0, 0, 0, 0, 0, 0 });
            msg.add_part(PdmRequest.BolusSchedule, commandBody);
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
            ise |= (ushort)(repeat << 12);
            if (alternate)
                ise |= 0x0800;
            return ise;
        }

        private static ushort getRepeatCount(ushort pulse, ushort[] others)
        {
            ushort repeatCount = 0;
            foreach (var other in others)
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
            for (int i = 0; i < table.Length; i++)
            {
                var v = table[i];
                body[i * 2] = (byte)(v >> 8);
                body[i * 2 + 1] = (byte)(v & 0xff);
            }
            return body;
        }

        private static ushort getChecksum(byte[] body)
        {
            ushort checksum = 0;
            foreach (byte b in body)
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

        private static Tuple<ushort, uint, int[]>[] getPulseIntervalEntries(decimal[] halfHourUnits)
        {
            var count = halfHourUnits.Length;
            var list1 = new Tuple<ushort, uint>[count];
            for (int i = 0; i < count; i++)
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

            for (int i = 0; i < count; i++)
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
                        list2.Add(new Tuple<ushort, uint, int[]>(subTotalPulses, lastPulseInterval, hh_indices.ToArray()));
                        subTotalPulses = pulses;
                        hh_indices = new List<int>(new int[] { i });
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

        public static PdmMessage request_set_basal_schedule(decimal[] schedule, ushort hour, ushort minute, ushort second)
        {
            var halved_schedule = new decimal[48];
            for (int i = 0; i < 47; i++)
                halved_schedule[i] = schedule[i] / 2m;

            int current_hh = hour * 2;
            ushort seconds_past_hh = 0;
            if (minute < 30)
            {
                seconds_past_hh = (ushort)(minute * 60 + second);
            }
            else
            {
                seconds_past_hh = (ushort)((minute - 30) * 60 + second);
            }

            var seconds_to_hh = (ushort)(1800 - seconds_past_hh);
            var seconds_to_hh8 = (ushort)(seconds_to_hh * 8);

            var pulse_list = getPulsesForHalfHours(halved_schedule);
            var ise_list = getInsulinScheduleTableFromPulses(pulse_list);
            var ise_body = getBodyFromTable(ise_list);
            var pulse_body = getBodyFromTable(pulse_list);

            var command_body = new byte[] { 0 };
            var body_checksum = new byte[] { (byte)current_hh };

            var current_hh_pulse_count = pulse_list[current_hh];
            var remaining_pulse_count = (ushort)(current_hh_pulse_count * seconds_to_hh / 1800);

            body_checksum.Append(seconds_to_hh8.ToBytes());
            body_checksum.Append(remaining_pulse_count.ToBytes());

            command_body.Append(getChecksum(body_checksum.Append(pulse_body)).ToBytes());
            command_body.Append(body_checksum);
            command_body.Append(ise_body);

            var msg = new PdmMessage(PdmRequest.InsulinSchedule, command_body);

            command_body = new byte[] { 0 };

            var pulse_entries = getPulseIntervalEntries(halved_schedule);
            for (int i = 0; i < pulse_entries.Length; i++)
            {
                var pti = pulse_entries[i];
                var pulses10 = pti.Item1;
                var interval = pti.Item2;
                var indices = pti.Item3;

                var ii = Array.IndexOf<int>(indices, current_hh);
                if (ii >= 0)
                {
                    command_body.Append((byte)i);
                    var pulses_past_intervals = (ushort)((uint)ii * (uint)1800000000 / (uint)interval);
                    var pulses_past_this_interval = (ushort)((uint)seconds_past_hh * (uint)1000000 / (uint)interval + 1);
                    var remaining_pulses_this_interval =(ushort)( pulses10 - pulses_past_this_interval - pulses_past_intervals);
                    var microseconds_to_next_interval = (uint)interval - ((uint)seconds_past_hh * (uint)1000000 % (uint)interval);

                    command_body.Append(remaining_pulses_this_interval.ToBytes());
                    command_body.Append(microseconds_to_next_interval.ToBytes());
                    break;
                }
            }

            for (int i = 0; i < pulse_entries.Length; i++)
            {
                var pti = pulse_entries[i];
                var pulses10 = pti.Item1;
                var interval = pti.Item2;

                command_body.Append(pulses10.ToBytes());
                command_body.Append(interval.ToBytes());
            }

            msg.add_part(PdmRequest.BasalSchedule, command_body);
            return msg;
        }

        public static void response_parse(PodMessage response, Pod pod)
        {
            pod.nonce_syncword = null;
            var parts = response.get_parts();
            foreach(var part in parts)
            {
                var response_type = (PodResponse)part.Item1;
                var response_body = part.Item2;

                switch(response_type)
                {
                    case PodResponse.VersionInfo:
                        parse_version_response(response_body, pod);
                        break;
                    case PodResponse.DetailInfo:
                        parse_information_response(response_body, pod);
                        break;
                    case PodResponse.ResyncRequest:
                        parse_resync_response(response_body, pod);
                        break;
                    case PodResponse.Status:
                        parse_status_response(response_body, pod);
                        break;
                    default:
                        throw new ProtocolError($"Unknown response type {response_type}");
                }
            }
        }

        private static void parse_version_response(byte[] response, Pod pod)
        {
            pod.state_last_updated = DateTime.UtcNow;
            if (response.Length == 27)
            {
                pod.id_version_unknown_7_bytes = response.Sub(0, 7);
                response = response.Sub(7);
            }

            var mx = response[0];
            var my = response[1];
            var mz = response[2];
            pod.id_version_pm = $"{mx}.{my}.{mz}";

            var ix = response[3];
            var iy = response[4];
            var iz = response[5];
            pod.id_version_pi = $"{ix}.{iy}.{iz}";

            pod.id_version_unknown_byte = response[6];
            pod.state_progress = (PodProgress)(response[7] & 0x0F);
            pod.id_lot = response.GetUInt32(8);
            pod.id_t = response.GetUInt32(12);

            if (response.Length == 21)
            {
                pod.radio_low_gain = response[17] >> 6;
                pod.radio_rssi = response[17] & 0b00111111;
                pod.radio_address = response.GetUInt32(17);
            }
            else
                pod.radio_address = response.GetUInt32(16);
        }

        private static void parse_information_response(byte[] response, Pod pod)
        {
            switch(response[0])
            {
                case 0x01:
                    pod.state_alerts = new ushort[]
                    {
                        response.GetUInt16(3),
                        response.GetUInt16(5),
                        response.GetUInt16(7),
                        response.GetUInt16(9),
                        response.GetUInt16(11),
                        response.GetUInt16(13),
                        response.GetUInt16(15),
                        response.GetUInt16(17)
                    };
                    break;
                case 0x02:
                    pod.state_last_updated = DateTime.UtcNow;
                    pod.state_faulted = true;
                    pod.state_progress = (PodProgress)response[1];
                    parse_delivery_state(pod, response[2]);
                    pod.insulin_canceled = response.GetUInt16(3) * 0.05m;
                    pod.radio_message_sequence = response[5];
                    pod.insulin_delivered = response.GetUInt16(6) * 0.05m;
                    pod.fault_event = response[8];
                    pod.fault_event_rel_time = response.GetUInt16(9);
                    pod.insulin_reservoir = response.GetUInt16(11) * 0.05m;
                    pod.state_active_minutes = response.GetUInt16(13);
                    pod.state_alert = response[15];
                    pod.fault_table_access = response[16];
                    pod.fault_insulin_state_table_corruption = response[17] >> 7;
                    pod.fault_internal_variables = (response[17] & 0x60) >> 6;
                    pod.fault_immediate_bolus_in_progress = (response[17] & 0x10) > 0;
                    pod.fault_progress_before = (PodProgress)(response[17] & 0x0F);
                    pod.radio_low_gain = (response[18] & 0xC0) >> 6;
                    pod.radio_rssi = response[18] & 0x3F;
                    pod.fault_progress_before2 = (PodProgress)(response[19] & 0x0F);
                    pod.fault_information_type2_last_word = response.GetUInt16(20);
                    break;
                default:
                    throw new ProtocolError($"Failed to parse the information response of type {response[0]}");
            }
        }

        private static void parse_delivery_state(Pod pod, byte delivery_state)
        {
            if ((delivery_state & 8) > 0)
                pod.state_bolus = BolusState.Extended;
            else if ((delivery_state & 4) > 0)
                pod.state_bolus = BolusState.Immediate;
            else
                pod.state_bolus = BolusState.NotRunning;

            if ((delivery_state & 2) > 0)
                pod.state_basal = BasalState.TempBasal;
            else if ((delivery_state & 1) > 0)
                pod.state_basal = BasalState.Program;
            else
                pod.state_basal = BasalState.NotRunning;
        }

        private static void parse_resync_response(byte[] response, Pod pod)
        {
            if (response[0] == 0x14)
                pod.nonce_syncword = response.GetUInt16(1);
            else
                throw new ProtocolError($"Unknown resync request {response[0]} from pod");
        }

        private static void parse_status_response(byte[] response, Pod pod)
        {
            pod.state_last_updated = DateTime.UtcNow;
            var s0 = response[0];
            uint s1 = response.GetUInt32(1);
            uint s2 = response.GetUInt32(5);

            parse_delivery_state(pod, (byte)(s0 >> 4));
            pod.state_progress = (PodProgress)(s0 & 0xF);

            pod.radio_message_sequence = (int)(s1 & 0x00007800) >> 11;
            pod.insulin_delivered = ((s1 & 0x0FFF8000) >> 15) * 0.05m;
            pod.insulin_canceled = (s1 & 0x000007FF) * 0.05m;
            pod.state_faulted = ((s2 >> 31) != 0);
            pod.state_alert = (byte) (s2 >> 23) & 0xFF;
            pod.state_active_minutes = (uint)((s2 & 0x007FFC00) >> 10);
            pod.insulin_reservoir = (s2 & 0x000003FF) * 0.05m;
        }
    }
}

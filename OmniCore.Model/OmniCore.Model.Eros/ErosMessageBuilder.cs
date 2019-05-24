using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosMessageBuilder : IMessageBuilder
    {

        private Bytes MessageBody = new Bytes();
        private List<IMessagePart> Parts = new List<IMessagePart>();

        public IMessage Build()
        {
            return new ErosMessage() { parts = Parts };
        }

        public IMessageBuilder WithPart(IMessagePart request)
        {
            Parts.Add(request);
            return this;
        }

        public IMessageBuilder WithAssignAddress(uint address)
        {
            return WithPart(new ErosRequest(PartType.RequestAssignAddress, new Bytes(address)));
        }

        public IMessageBuilder WithSetupPod(uint lot, uint tid, uint address,
            int year, byte month, byte day, byte hour, byte minute)
        {
            var cmd_body = new Bytes();
            cmd_body.Append(address);
            cmd_body.Append(new byte[] { 0x14, 0x04 });
            cmd_body.Append(new byte[] { month, day, (byte)(year - 2000), hour, minute });
            cmd_body.Append(lot);
            cmd_body.Append(tid);
            return WithPart(new ErosRequest(PartType.RequestSetupPod, cmd_body));
        }

        public IMessageBuilder WithAlertSetup(List<AlertConfiguration> alert_configurations)
        {
            var cmd_body = new Bytes();
            foreach (var ac in alert_configurations)
            {
                if (ac.alert_after_minutes == null && ac.alert_after_reservoir == null && ac.activate)
                    throw new PdmException("Either alert_after_minutes or alert_after_reservoir must be set");
                else if (ac.alert_after_minutes != null && ac.alert_after_reservoir != null)
                    throw new PdmException("Only one of alert_after_minutes or alert_after_reservoir must be set");

                if (ac.alert_duration > 0x1FF)
                    throw new PdmException($"Alert duration in minutes cannot be more than {0x1ff:%d}");
                else if (ac.alert_duration < 0)
                    throw new PdmException("Invalid alert duration value");

                if (ac.alert_after_minutes != null && ac.alert_after_minutes > 4800)
                    throw new PdmException("Alert cannot be set beyond 80 hours");
                if (ac.alert_after_minutes != null && ac.alert_after_minutes < 0)
                    throw new PdmException("Invalid value for alert_after_minutes");

                if (ac.alert_after_reservoir != null && ac.alert_after_reservoir > 50)
                    throw new PdmException("Alert cannot be set for more than 50 units");
                if (ac.alert_after_reservoir != null && ac.alert_after_reservoir < 0)
                    throw new PdmException("Invalid value for alert_after_reservoir");

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
            return WithPart(new ErosRequest(PartType.RequestConfigureAlerts, cmd_body, true));
        }

        public IMessageBuilder WithStatus(StatusRequestType statusRequestType = StatusRequestType.Standard)
        {
            return WithPart(new ErosRequest(PartType.RequestStatus, new Bytes().Append((byte)statusRequestType)));
        }

        public IMessageBuilder WithAcknowledgeAlerts(byte alert_mask)
        {
            return WithPart(new ErosRequest(PartType.RequestAcknowledgeAlerts, new Bytes().Append(alert_mask), true));
        }

        public IMessageBuilder WithDeactivate()
        {
            return WithPart(new ErosRequest(PartType.RequestDeactivatePod, new Bytes(), true));
        }

        public IMessageBuilder WithDeliveryFlags(byte byte16, byte byte17)
        {
            return WithPart(new ErosRequest(PartType.RequestSetDeliveryFlags, new Bytes().Append(byte16).Append(byte17), true));
        }

        public IMessageBuilder WithCancelBolus()
        {
            return WithPart(new ErosRequest(PartType.RequestCancelDelivery, new Bytes().Append(0x04), true));
        }

        public IMessageBuilder WithCancelTempBasal()
        {
            return WithPart(new ErosRequest(PartType.RequestCancelDelivery, new Bytes().Append(0x02), true));
        }

        public IMessageBuilder WithStopBasalInsulin()
        {
            return WithPart(new ErosRequest(PartType.RequestCancelDelivery, new Bytes().Append(0x01), true));
        }

        public IMessageBuilder WithTempBasal(decimal basal_rate_iuhr, decimal duration_hours)
        {
            var half_hour_count = (int)(duration_hours * 2.0m);
            var hh_units = new decimal[half_hour_count];
            for (int i = 0; i < half_hour_count; i++)
                hh_units[i] = basal_rate_iuhr / 2.0m;

            var pulseList = getPulsesForHalfHours(hh_units);
            var iseList = getInsulinScheduleTableFromPulses(pulseList);

            var iseBody = getBodyFromTable(iseList);
            var pulseBody = getBodyFromTable(pulseList);

            var cmd_body = new Bytes();
            cmd_body.Append(0x01);

            var body_checksum = new Bytes();
            body_checksum.Append((byte)half_hour_count);
            ushort b1 = 0x3840;
            body_checksum.Append(b1);
            body_checksum.Append(pulseList[0]);
            var checksum = getChecksum(new Bytes(body_checksum, pulseBody));

            cmd_body.Append(checksum);
            cmd_body.Append(body_checksum);
            cmd_body.Append(iseBody);

            WithPart(new ErosRequest(PartType.RequestInsulinSchedule, cmd_body, true));

            byte reminders = 0;
            //#if confidenceReminder:
            //# reminders |= 0x40

            cmd_body = new Bytes();
            cmd_body.Append(reminders).Append(0x00);
            var pulseEntries = getPulseIntervalEntries(hh_units);

            var firstPte = pulseEntries[0];
            cmd_body.Append(firstPte.Item1);
            cmd_body.Append(firstPte.Item2);

            foreach (var pte in pulseEntries)
            {
                cmd_body.Append(pte.Item1);
                cmd_body.Append(pte.Item2);
            }

            return WithPart(new ErosRequest(PartType.RequestTempBasalSchedule, cmd_body));
        }

        public IMessageBuilder WithPrimeCannula()
        {
            return WithImmediatePulses(52, 8, 1);
        }

        public IMessageBuilder WithInserCannula()
        {
            return WithImmediatePulses(10, 8, 1);
        }

        public IMessageBuilder WithBolus(decimal iu_bolus)
        {
            return WithImmediatePulses((ushort)(iu_bolus / 0.05m));
        }

        private IMessageBuilder WithImmediatePulses(ushort pulse_count,
        int pulse_speed = 16, int delivery_delay = 2)
        {
            var commandBody = new Bytes().Append(0x02);
            var bodyForChecksum = new Bytes().Append(0x01);
            var pulse_span = (ushort)(pulse_speed * pulse_count);
            bodyForChecksum.Append(pulse_span);
            bodyForChecksum.Append(pulse_count);
            bodyForChecksum.Append(pulse_count);
            var checksum = getChecksum(bodyForChecksum);
            commandBody.Append(checksum);
            commandBody.Append(bodyForChecksum);

            WithPart(new ErosRequest(PartType.RequestInsulinSchedule, commandBody, true));

            commandBody = new Bytes().Append(0x00);
            ushort p10 = (ushort)(pulse_count * 10);
            commandBody.Append(p10);
            uint dd = (uint)delivery_delay * (uint)100000;
            commandBody.Append(dd);
            commandBody.Append(new byte[] { 0, 0, 0, 0, 0, 0 });
            return WithPart(new ErosRequest(PartType.RequestBolusSchedule, commandBody));
        }

        private ushort[] getPulsesForHalfHours(decimal[] halfHourUnits)
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

        private ushort getIse(ushort pulses, ushort repeat, bool alternate)
        {
            var ise = (ushort)(pulses & 0x03ff);
            ise |= (ushort)(repeat << 12);
            if (alternate)
                ise |= 0x0800;
            return ise;
        }

        private ushort getRepeatCount(ushort pulse, ushort[] others)
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

        private ushort[] SubBytes(ushort[] source, int startIndex)
        {
            var ret = new ushort[source.Length - startIndex];
            Buffer.BlockCopy(source, startIndex, ret, 0, ret.Length);
            return ret;
        }

        private ushort[] getInsulinScheduleTableFromPulses(ushort[] pulses)
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

                var alternatingTable = SubBytes(pulses, ptr);
                for (int k = 1; k < alternatingTable.Length; k += 2)
                    alternatingTable[k] -= 1;

                var pulse = alternatingTable[0];
                var others = SubBytes(alternatingTable, 1);
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
                    others = SubBytes(pulses, ptr + 1);
                    repeats = getRepeatCount(pulse, others);
                    if (repeats > 15)
                        repeats = 15;
                    iseTable.Add(getIse(pulse, repeats, false));
                }
                ptr += repeats + 1;
            }
            return iseTable.ToArray();
        }

        private Bytes getBodyFromTable(ushort[] table)
        {
            byte[] body = new byte[table.Length * 2];
            for (int i = 0; i < table.Length; i++)
            {
                var v = table[i];
                body[i * 2] = (byte)(v >> 8);
                body[i * 2 + 1] = (byte)(v & 0xff);
            }
            return new Bytes(body);
        }

        private ushort getChecksum(Bytes body)
        {
            ushort checksum = 0;
            foreach (byte b in body.ToArray())
            {
                checksum += b;
            }
            return checksum;
        }

        private int getHalfHourPulseInterval(uint pulseCount)
        {
            if (pulseCount == 0)
                return 180000000;
            else
                return (int)(180000000 / pulseCount);
        }

        private Tuple<ushort, uint, int[]>[] getPulseIntervalEntries(decimal[] halfHourUnits)
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

        public IMessageBuilder WithBasalSchedule(decimal[] schedule, ushort hour, ushort minute, ushort second)
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

            var command_body = new Bytes(0);
            var body_checksum = new Bytes((byte)current_hh);

            var current_hh_pulse_count = pulse_list[current_hh];
            var remaining_pulse_count = (ushort)(current_hh_pulse_count * seconds_to_hh / 1800);

            body_checksum.Append(seconds_to_hh8);
            body_checksum.Append(remaining_pulse_count);

            command_body.Append(getChecksum(new Bytes(body_checksum, pulse_body)));
            command_body.Append(body_checksum);
            command_body.Append(ise_body);

            WithPart(new ErosRequest(PartType.RequestInsulinSchedule, command_body, true));

            command_body = new Bytes(new byte[] { 0, 0 });

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
                    var remaining_pulses_this_interval = (ushort)(pulses10 - pulses_past_this_interval - pulses_past_intervals);
                    var microseconds_to_next_interval = (uint)interval - ((uint)seconds_past_hh * (uint)1000000 % (uint)interval);

                    command_body.Append(remaining_pulses_this_interval);
                    command_body.Append(microseconds_to_next_interval);
                    break;
                }
            }

            for (int i = 0; i < pulse_entries.Length; i++)
            {
                var pti = pulse_entries[i];
                var pulses10 = pti.Item1;
                var interval = pti.Item2;

                command_body.Append(pulses10);
                command_body.Append(interval);
            }

            return WithPart(new ErosRequest(PartType.RequestBasalSchedule, command_body));
        }
    }
}

using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;

namespace OmniCore.Model.Eros
{
    public class ErosResponse : IMessagePart
    {
        public PartType PartType { get; set;  }

        public Bytes PartData { get; set; }

        public void Parse(IPod pod)
        {
            switch (PartType)
            {
                case PartType.ResponseVersionInfo:
                    parse_version_response(pod);
                    break;
                case PartType.ResponseDetailInfoResponse:
                    parse_information_response(pod);
                    break;
                case PartType.ResponseResyncResponse:
                    parse_resync_response(pod);
                    break;
                case PartType.ResponseStatus:
                    parse_status_response(pod);
                    break;
                default:
                    throw new OmniCoreException($"Unknown response type {PartType}");
            }
        }

        private void parse_version_response(IPod pod)
        {
            bool lengthyResponse = false;
            pod.state_last_updated = DateTime.UtcNow;
            int i = 1;
            if (PartData.Length == 27)
            {
                pod.id_version_unknown_7_bytes = PartData.ToArray(i, i + 7);
                i += 7;
                lengthyResponse = true;
            }

            var mx = PartData.Byte(i++);
            var my = PartData.Byte(i++);
            var mz = PartData.Byte(i++);
            pod.id_version_pm = $"{mx}.{my}.{mz}";

            var ix = PartData.Byte(i++);
            var iy = PartData.Byte(i++);
            var iz = PartData.Byte(i++);
            pod.id_version_pi = $"{ix}.{iy}.{iz}";

            pod.id_version_unknown_byte = PartData.Byte(i++);
            pod.state_progress = (PodProgress)(PartData.Byte(i++) & 0x0F);
            pod.id_lot = PartData.DWord(i);
            pod.id_t = PartData.DWord(i + 4);
            i += 8;
            if (!lengthyResponse)
            {
                var rb = PartData.Byte(i++);
                pod.radio_low_gain = rb >> 6;
                pod.radio_rssi = rb & 0b00111111;
                pod.radio_address = PartData.DWord(i);
            }
            else
                pod.radio_address = PartData.DWord(i);
        }

        private void parse_information_response(IPod pod)
        {
            int i = 1;
            var rt = PartData.Byte(i++);
            switch (rt)
            {
                case 0x01:
                    pod.state_alert_w278 = PartData.Word(i);
                    i += 2;
                    pod.state_alerts = new ushort[]
                    {
                        PartData.Word(i),
                        PartData.Word(i + 2),
                        PartData.Word(i + 4),
                        PartData.Word(i + 6),
                        PartData.Word(i + 8),
                        PartData.Word(i + 10),
                        PartData.Word(i + 12),
                        PartData.Word(i + 14),
                    };
                    break;
                case 0x02:
                    pod.state_last_updated = DateTime.UtcNow;
                    pod.state_faulted = true;
                    pod.state_progress = (PodProgress)PartData.Byte(i++);
                    parse_delivery_state(pod, PartData.Byte(i++));
                    pod.insulin_canceled = PartData.Byte(i++) * 0.05m;
                    pod.radio_message_sequence = PartData.Byte(i++);
                    pod.insulin_delivered = PartData.Byte(i++) * 0.05m;
                    pod.fault_event = PartData.Byte(i++);
                    pod.fault_event_rel_time = PartData.Word(i);
                    pod.insulin_reservoir = PartData.Word(i + 2) * 0.05m;
                    pod.state_active_minutes = PartData.Word(i + 4);
                    i += 6;
                    pod.state_alert = PartData.Byte(i++);
                    pod.fault_table_access = PartData.Byte(i++);
                    byte f17 = PartData.Byte(i++);
                    pod.fault_insulin_state_table_corruption = f17 >> 7;
                    pod.fault_internal_variables = (f17 & 0x60) >> 6;
                    pod.fault_immediate_bolus_in_progress = (f17 & 0x10) > 0;
                    pod.fault_progress_before = (PodProgress)(f17 & 0x0F);
                    byte r18 = PartData.Byte(i++);
                    pod.radio_low_gain = (r18 & 0xC0) >> 6;
                    pod.radio_rssi = r18 & 0x3F;
                    pod.fault_progress_before2 = (PodProgress)(PartData.Byte(i++) & 0x0F);
                    pod.fault_information_type2_last_word = PartData.Byte(i++);
                    break;
                default:
                    throw new OmniCoreException($"Failed to parse the information response of type {rt}");
            }
        }

        private void parse_delivery_state(IPod pod, byte delivery_state)
        {
            if ((delivery_state & 8) > 0)
                pod.state_bolus = BolusState.Extended;
            else if ((delivery_state & 4) > 0)
                pod.state_bolus = BolusState.Immediate;
            else
                pod.state_bolus = BolusState.Inactive;

            if ((delivery_state & 2) > 0)
                pod.state_basal = BasalState.Temporary;
            else if ((delivery_state & 1) > 0)
                pod.state_basal = BasalState.Scheduled;
            else
                pod.state_basal = BasalState.Suspended;
        }

        private void parse_resync_response(IPod pod)
        {
            if (PartData[1] == 0x14)
                pod.nonce_syncword = PartData.Word(2);
            else
                throw new OmniCoreException($"Unknown resync request {PartData} from pod");
        }

        private void parse_status_response(IPod pod)
        {
            pod.state_last_updated = DateTime.UtcNow;
            var s0 = PartData[0];
            uint s1 = PartData.DWord(1);
            uint s2 = PartData.DWord(5);

            parse_delivery_state(pod, (byte)(s0 >> 4));
            pod.state_progress = (PodProgress)(s0 & 0xF);

            pod.radio_message_sequence = (int)(s1 & 0x00007800) >> 11;
            pod.insulin_delivered = ((s1 & 0x0FFF8000) >> 15) * 0.05m;
            pod.insulin_canceled = (s1 & 0x000007FF) * 0.05m;
            pod.state_faulted = ((s2 >> 31) != 0);
            pod.state_alert = (byte)((s2 >> 23) & 0xFF);
            pod.state_active_minutes = (uint)((s2 & 0x007FFC00) >> 10);
            pod.insulin_reservoir = (s2 & 0x000003FF) * 0.05m;
        }
    }
}

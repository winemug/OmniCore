using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Diagnostics;

namespace OmniCore.Model.Eros
{
    public class ErosResponse : IMessagePart
    {
        public PartType PartType { get; set;  }

        public Bytes PartData { get; set; }

        public void Parse(IPod pod)
        {
            Debug.WriteLine($"Parsing response type {PartType}");
            switch (PartType)
            {
                case PartType.ResponseVersionInfo:
                    parse_version_response(pod);
                    break;
                case PartType.ResponseDetailInfoResponse:
                    parse_information_response(pod);
                    break;
                case PartType.ResponseResyncResponse:
                    parse_resync_response(pod as ErosPod);
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
            pod.Created = DateTime.UtcNow;
            int i = 1;
            if (PartData.Length == 27)
            {
                pod.VersionUnknown = PartData.ToHex(i, i + 7);
                i += 7;
                lengthyResponse = true;
            }

            var mx = PartData.Byte(i++);
            var my = PartData.Byte(i++);
            var mz = PartData.Byte(i++);
            pod.VersionPm = $"{mx}.{my}.{mz}";

            var ix = PartData.Byte(i++);
            var iy = PartData.Byte(i++);
            var iz = PartData.Byte(i++);
            pod.VersionPi = $"{ix}.{iy}.{iz}";

            pod.VersionUnknown += PartData.ToHex(i++, i);

            pod.Status = new ErosPodStatus() { Created = DateTime.UtcNow };
            pod.Status.Progress = (PodProgress)(PartData.Byte(i++) & 0x0F);

            pod.Lot = PartData.DWord(i);
            pod.Serial = PartData.DWord(i + 4);
            i += 8;
            if (!lengthyResponse)
            {
                var rb = PartData.Byte(i++);
                pod.RadioIndicators = new ErosPodRadioIndicators();
                pod.RadioIndicators.RadioLowGain = rb >> 6;
                pod.RadioIndicators.RadioRssi = rb & 0b00111111;
                pod.RadioAddress = PartData.DWord(i);
            }
            else
                pod.RadioAddress = PartData.DWord(i);
        }

        private void parse_information_response(IPod pod)
        {
            int i = 0;
            var rt = PartData.Byte(i++);
            switch (rt)
            {
                case 0x01:
                    pod.AlertStates = new ErosPodAlertStates();
                    pod.AlertStates.AlertW278 = PartData.Word(i);
                    i += 2;
                    pod.AlertStates.AlertStates = new uint[]
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
                    pod.Status = new ErosPodStatus();
                    pod.Status.Created = DateTime.UtcNow;
                    pod.Status.Faulted = true;
                    pod.Status.Progress = (PodProgress)PartData.Byte(i++);
                    parse_delivery_state(pod, PartData.Byte(i++));
                    pod.Status.NotDeliveredInsulin = PartData.Byte(i++) * 0.05m;
                    pod.Status.MessageSequence = PartData.Byte(i++);
                    pod.Status.DeliveredInsulin = PartData.Byte(i++) * 0.05m;

                    pod.Fault = new ErosPodFault();
                    pod.Fault.FaultCode = PartData.Byte(i++);
                    pod.Fault.FaultRelativeTime = PartData.Word(i);

                    pod.Status.Reservoir = PartData.Word(i + 2) * 0.05m;
                    pod.Status.ActiveMinutes = PartData.Word(i + 4);
                    i += 6;
                    pod.Status.AlertMask = PartData.Byte(i++);
                    pod.Fault.TableAccessFault = PartData.Byte(i++);
                    byte f17 = PartData.Byte(i++);
                    pod.Fault.InsulinStateTableCorruption = f17 >> 7;
                    pod.Fault.InternalFaultVariables = (f17 & 0x60) >> 6;
                    pod.Fault.FaultedWhileImmediateBolus = (f17 & 0x10) > 0;
                    pod.Fault.ProgressBeforeFault = (PodProgress)(f17 & 0x0F);
                    byte r18 = PartData.Byte(i++);

                    pod.RadioIndicators = new ErosPodRadioIndicators();
                    pod.RadioIndicators.RadioLowGain = (r18 & 0xC0) >> 6;
                    pod.RadioIndicators.RadioRssi = r18 & 0x3F;

                    pod.Fault.ProgressBeforeFault2 = (PodProgress)(PartData.Byte(i++) & 0x0F);
                    pod.Fault.FaultInformation2LastWord = PartData.Byte(i++);
                    break;
                default:
                    throw new OmniCoreException($"Failed to parse the information response of type {rt}");
            }
        }

        private void parse_delivery_state(IPod pod, byte delivery_state)
        {
            if ((delivery_state & 8) > 0)
                pod.Status.BolusState = BolusState.Extended;
            else if ((delivery_state & 4) > 0)
                pod.Status.BolusState = BolusState.Immediate;
            else
                pod.Status.BolusState = BolusState.Inactive;

            if ((delivery_state & 2) > 0)
                pod.Status.BasalState = BasalState.Temporary;
            else if ((delivery_state & 1) > 0)
                pod.Status.BasalState = BasalState.Scheduled;
            else
                pod.Status.BasalState = BasalState.Suspended;
        }

        private void parse_resync_response(ErosPod pod)
        {
            if (PartData[1] == 0x14)
                pod.RuntimeVariables.NonceSync = PartData.Word(2);
            else
                throw new OmniCoreException($"Unknown resync request {PartData} from pod");
        }

        private void parse_status_response(IPod pod)
        {
            pod.Status = new ErosPodStatus();
            pod.Status.Created = DateTime.UtcNow;
            var s0 = PartData[0];
            uint s1 = PartData.DWord(1);
            uint s2 = PartData.DWord(5);

            parse_delivery_state(pod, (byte)(s0 >> 4));
            pod.Status.Progress = (PodProgress)(s0 & 0xF);

            pod.Status.MessageSequence = (int)(s1 & 0x00007800) >> 11;
            pod.Status.DeliveredInsulin = ((s1 & 0x0FFF8000) >> 15) * 0.05m;
            pod.Status.NotDeliveredInsulin = (s1 & 0x000007FF) * 0.05m;
            pod.Status.Faulted = ((s2 >> 31) != 0);
            pod.Status.AlertMask = (byte)((s2 >> 23) & 0xFF);
            pod.Status.ActiveMinutes = (uint)((s2 & 0x007FFC00) >> 10);
            pod.Status.Reservoir = (s2 & 0x000003FF) * 0.05m;
        }
    }
}

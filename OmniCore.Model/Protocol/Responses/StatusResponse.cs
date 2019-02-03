using OmniCore.Model.Enums;
using OmniCore.Model.Protocol.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol.Responses
{
    public class StatusResponse : Message
    {
        public BasalState BasalState { get; private set; }
        public BolusState BolusState { get; private set; }
        public PodProgress Progress { get; private set; }
        public int DeliveredPulses { get; private set; }
        public int NotDeliveredPulses { get; private set; }
        public int MessageSequence { get; private set; }
        public bool FaultEvent { get; private set; }
        public Alarm Alarms { get; private set; }
        public int ActiveMinutes { get; private set; }
        public int Reservoir { get; private set; }

        // https://github.com/openaps/openomni/wiki/Command-1D-Status-response
        public StatusResponse(IEnumerable<IMessagePart> parts)
        {
            base.Parts = parts;

            IMessagePart part = null;
            foreach (var p in parts)
            {
                if (part != null)
                    throw new ParserException("Status response contains more than one part!");
                part = p;
            }

            if (part.Content.Length != 9)
                throw new ParserException("Status response has invalid length");

            var b0 = part.Content[0];

            //      b0 => bits abcdeeee
            //      4 - bit abcd nibble is a bit mask for active insulin delivery
            //      a 8: Extended bolus active, exclusive of 4 bit
            //      b 4: Immediate bolus active, exclusive of 8 bit
            //      c 2: Temp basal active, exclusive of 1 bit
            //      d 1: Basal active, exclusive of 2 bit
            //      4 - bit eeee nibble is the Pod Progress State value(0 thru to F)

            var extendedBolusActive = (b0 & 0x80) > 0;
            var immediateBolusActive = (b0 & 0x40) > 0;
            var tempBasalActive = (b0 & 0x20) > 0;
            var basalActive = (b0 & 0x10) > 0;

            if (extendedBolusActive && immediateBolusActive)
                throw new ParserException("Status response reports inconsistent state: Both extended and immediate bolus active");

            if (tempBasalActive && basalActive)
                throw new ParserException("Status response reports inconsistent state: Both temporary and scheduled basal active");

            if (extendedBolusActive)
                this.BolusState = BolusState.ExtendedBolus;
            else if (immediateBolusActive)
                this.BolusState = BolusState.Running;
            else
                this.BolusState = BolusState.Inactive;

            if (tempBasalActive)
                this.BasalState = BasalState.Temporary;
            else if (basalActive)
                this.BasalState = BasalState.Scheduled;
            else
                this.BasalState = BasalState.Suspended;

            this.Progress = (PodProgress)(b0 & 0x0F);

            var i1 = part.Content.GetUInt32BigEndian(1);
            //i1 > 0PPPSNNN dword = 0000 pppp pppp pppp psss snnn nnnn nnnn
            //0000 4 zero bits
            //ppppppppppppp 13 bits, Total 0.05U insulin pulses
            //ssss 4 bits, message sequence number(saved B9 >> 2)
            //nnn nnnn nnnn 11 bits, 0.05U Insulin pulses not delivered if cancelled by user
            uint maskDelivered = 0b00001111111111111000000000000000;
            uint maskSequence = 0b10000000000000000111100000000000;
            uint maskCancelled = 0b10000000000000000000011111111111;

            this.DeliveredPulses = (int)((i1 & maskDelivered) >> 15);
            this.MessageSequence = (int)((i1 & maskSequence) >> 11);
            this.NotDeliveredPulses = (int)(i1 & maskCancelled);

            var i5 = part.Content.GetUInt32BigEndian(5);
            //i5 > dword = faaa aaaa attt tttt tttt ttrr rrrr rrrr
            //f 1 bit, 0 or 1 if the Pod has encountered fault event $14
            //aaaaaaaa 8 bits, bit mask of the active, unacknowledged alerts (1 << alert #) from the Command 19 Configure Alerts; this bit mask is the same as the TT byte in the 02 Error Response Type 2
            //ttttttttttttt 13 bits, Pod active time in minutes
            //rrrrrrrrrr 10 bits, Reservoir 0.05U pulses remaining(if <= 50U) or $3ff(if > 50U left)

            uint maskFaultEvent = 0b10000000000000000000000000000000;
            uint maskAlarms = 0b01111111100000000000000000000000;
            uint maskMinutes = 0b00000000011111111111110000000000;
            uint maskReservoir = 0b10000000000000000000001111111111;

            this.FaultEvent = (i5 & maskFaultEvent) != 0;
            this.Alarms = (Alarm)((i5 & maskAlarms) >> 23);
            this.ActiveMinutes = (int)((i5 & maskMinutes) >> 10);
            this.Reservoir = (int)(i5 & maskReservoir);
        }
    }
}

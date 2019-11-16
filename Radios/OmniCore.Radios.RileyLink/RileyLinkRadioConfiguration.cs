using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioConfiguration : IRadioConfiguration
    {
        // shifts in complements of 326.211 Hz
        public int FrequencyShift { get; set; } = 0;
        public DeviationRange Deviation { get; set; } = DeviationRange.Normal;

        public PowerAmplification Amplification { get; set; } = PowerAmplification.A4;

        // 0x00 to 0x1F
        public int RxIntermediateFrequency { get; set; } = 0x06;

        // 0x00 to 0x07
        public int PqeThreshold { get; set; } = 0x01;

        // 0x00 to 0x03
        public int FilterBWExponent { get; set; } = 3;

        // 0x00 to 0x03
        public int FilterBWDecimationRatio { get; set; } = 0;

        public bool DataWhitening { get; set; } = false;

        public bool PreambleCheckWithCarrierSense { get; set; } = true;

        // 0x00 to 0x07
        public int TxPreambleCountSetting { get; set; } = 0x04;

        public bool ForwardErrorCorrection { get; set; } = false;

        // 0x00 to 0x03
        public int RxAttenuationLevel { get; set; } = 0x00;

        public List<Tuple<RileyLinkRegister, int>> GetConfiguration()
        {
            var registers = new List<Tuple<RileyLinkRegister, int>>();
            registers.Add(Tuple.Create(RileyLinkRegister.SYNC0, 0x5A));
            registers.Add(Tuple.Create(RileyLinkRegister.SYNC1, 0xA5));
            registers.Add(Tuple.Create(RileyLinkRegister.PKTLEN, 0x50));

            var frequency = (int)(433910000 / (24000000 / Math.Pow(2, 16)));
            frequency += FrequencyShift;
            registers.Add(Tuple.Create(RileyLinkRegister.FREQ0, frequency & 0xff));
            registers.Add(Tuple.Create(RileyLinkRegister.FREQ1, (frequency >> 8) & 0xff));
            registers.Add(Tuple.Create(RileyLinkRegister.FREQ2, (frequency >> 16) & 0xff));

            switch(Deviation)
            {
                case DeviationRange.Normal:
                    registers.Add(Tuple.Create(RileyLinkRegister.DEVIATN, 0x44));
                    break;
                case DeviationRange.Wide:
                    registers.Add(Tuple.Create(RileyLinkRegister.DEVIATN, 0x45));
                    break;
                case DeviationRange.Narrow:
                    registers.Add(Tuple.Create(RileyLinkRegister.DEVIATN, 0x43));
                    break;
            }

            registers.Add(Tuple.Create(RileyLinkRegister.FREND0, 0x00));
            registers.Add(Tuple.Create(RileyLinkRegister.PATABLE0, (int)Amplification));

            registers.Add(Tuple.Create(RileyLinkRegister.FSCTRL0, 0x00));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCTRL1, RxIntermediateFrequency));

            var pktctrl1 = PqeThreshold << 5;
            pktctrl1 &= 0xE0;

            var pktctrl0 = DataWhitening ? 0x40 : 0x00;

            registers.Add(Tuple.Create(RileyLinkRegister.PKTCTRL1, pktctrl1));
            registers.Add(Tuple.Create(RileyLinkRegister.PKTCTRL0, pktctrl0));

            var mcfg4 = FilterBWExponent << 6;
            mcfg4 |= FilterBWDecimationRatio << 4;
            mcfg4 &= 0xF0;
            mcfg4 |= 0x0A;
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG4, mcfg4));
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG3, 0xBC));

            var mcfg2 = PreambleCheckWithCarrierSense ? 0x06 : 0x02;
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG2, mcfg2));

            var mcfg1 = ForwardErrorCorrection ? 0x80 : 0x00;
            mcfg1 |= TxPreambleCountSetting << 4;
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG1, mcfg1));
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG0, 0xF8));

            var mcsm0 = 0x18 | RxAttenuationLevel;
            registers.Add(Tuple.Create(RileyLinkRegister.MCSM0, mcsm0));

            registers.Add(Tuple.Create(RileyLinkRegister.MCSM0, mcsm0));

            registers.Add(Tuple.Create(RileyLinkRegister.FOCCFG, 0x17));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCAL3, 0xE9));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCAL2, 0x2A));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCAL1, 0x00));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCAL0, 0x1F));
            registers.Add(Tuple.Create(RileyLinkRegister.TEST1, 0x35));
            registers.Add(Tuple.Create(RileyLinkRegister.TEST0, 0x09));

            return registers;
        }
    }

    public enum DeviationRange
    {
        Wide, Normal, Narrow
    }

    public enum PowerAmplification
    {
        A0 = 0x0E,
        A1 = 0x1D,
        A2 = 0x34,
        A3 = 0x2c,
        A4 = 0x60,
        A5 = 0x84,
        A6 = 0xC8
    }
}

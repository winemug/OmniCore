using System;
using System.Reflection;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class RadioOptions
    {

        public bool UseHardwareEncoding { get; set; } = false;

        // shifts in complements of 326.211 Hz
        public int RxFrequencyShift { get; set; } = 0;

        public int TxFrequencyShift { get; set; } = 0;

        public TransmissionPower Amplification { get; set; } = TransmissionPower.Normal;

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

        public bool SameAs(RadioOptions other)
        {
            var propertyInfos = typeof(RadioOptions)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var propertyInfo in propertyInfos)
            {
                var getter = propertyInfo.GetGetMethod();
                var v1 = getter.Invoke(this, null);
                var v2 = getter.Invoke(this, null);

                if (!v1.Equals(v2))
                    return false;
            }

            return true;
        }
    }
}
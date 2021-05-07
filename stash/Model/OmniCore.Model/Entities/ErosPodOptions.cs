using System;

namespace OmniCore.Model.Entities
{
    public class ErosPodOptions
    {
        public bool AutoAdjustPodTime { get; set; } = false;
        public bool DeactivateOnError { get; set; } = true;
        public int? ExecuteCommandRssiThreshold { get; set; } = 95;
        public bool BeepStartBolus { get; set; } = true;
        public bool BeepEndBolus { get; set; } = true;
        public bool BeepStartTempBasal { get; set; } = false;
        public bool BeepEndTempBasal { get; set; } = false;
        public TimeSpan? BeepIntervalTempBasalActive { get; set; } = null;
        public bool BeepStartExtendedBolus { get; set; } = false;
        public bool BeepEndExtendedBolus { get; set; } = false;
        public TimeSpan? BeepIntervalExtendedBolusActive { get; set; } = null;
        public TimeSpan StatusCheckIntervalGood { get; set; } = TimeSpan.FromMinutes(45);
        public TimeSpan StatusCheckIntervalBad { get; set; } = TimeSpan.FromMinutes(5);
    }
}
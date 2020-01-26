namespace OmniCore.Model.Entities
{
    public class PodOptions
    {
        public bool AutoAdjustPodTime { get; set; }
        public bool DeactivateOnError { get; set; }
        public int? ExecuteCommandRssiThreshold { get; set; }
        public bool BeepStartBolus { get; set; }
        public bool BeepEndBolus { get; set; }
        public bool BeepStartTempBasal { get; set; }
        public bool BeepEndTempBasal { get; set; }
        public decimal? BeepWhileTempBasalActive { get; set; }
        public bool BeepStartExtendedBolus { get; set; }
        public bool BeepEndExtendedBolus { get; set; }
        public decimal? BeepWhileExtendedBolusActive { get; set; }
    }
}
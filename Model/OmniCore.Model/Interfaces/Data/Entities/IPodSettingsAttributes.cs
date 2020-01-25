namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IPodSettingsAttributes
    {
        bool AutoAdjustPodTime { get; set; }
        bool DeactivateOnError { get; set; }
        int? ExecuteCommandRssiThreshold { get; set; }
        
        IReminderAttributes ExpiresSoonReminder { get; set; }
        IReminderAttributes ReservoirLowReminder { get; set; }
        IReminderAttributes ExpiredReminder { get; set; }

        bool BeepStartBolus { get; set; }
        bool BeepEndBolus { get; set; }

        bool BeepStartTempBasal { get; set; }
        bool BeepEndTempBasal { get; set; }
        decimal? BeepWhileTempBasalActive { get; set; }

        bool BeepStartExtendedBolus { get; set; }
        bool BeepEndExtendedBolus { get; set; }
        decimal? BeepWhileExtendedBolusActive { get; set; }
    }
}

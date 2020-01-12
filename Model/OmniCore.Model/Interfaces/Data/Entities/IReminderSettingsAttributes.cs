namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IReminderSettingsAttributes
    {
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

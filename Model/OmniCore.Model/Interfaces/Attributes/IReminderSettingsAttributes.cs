using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Attributes
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

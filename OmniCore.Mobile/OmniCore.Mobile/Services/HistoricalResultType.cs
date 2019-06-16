using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Services
{
    public enum HistoricalResultType
    {
        SetBasalSchedule,
        Bolus,
        CancelBolus,
        SetTempBasal,
        CancelTempBasal,
        StartExtendedBolus,
        StopExtendedBolus,
        Status
    }
}

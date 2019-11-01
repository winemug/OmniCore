using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Services
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

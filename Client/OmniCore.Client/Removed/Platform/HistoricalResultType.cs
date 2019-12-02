using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Platform
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

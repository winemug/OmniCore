using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Services
{
    public enum RemoteRequestType
    {
        SetBasalSchedule,
        Bolus,
        CancelBolus,
        CancelTempBasal,
        UpdateStatus,
        SetTempBasal
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Services
{
    public enum RemoteRequestType
    {
        SetProfile,
        Bolus,
        CancelBolus,
        SetTempBasal,
        CancelTempBasal,
        GetStatus
    }
}

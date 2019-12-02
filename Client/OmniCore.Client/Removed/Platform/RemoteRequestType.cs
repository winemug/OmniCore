using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Platform
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

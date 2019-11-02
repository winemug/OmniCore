using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Enums
{
    public enum ReminderSlot
    {
        NotActivated = 0,
        ExpiresSoon = 1,
        ReservoirLow = 2,
        Expired = 3,
        NotStarted = 7
    }
}

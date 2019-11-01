using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IReminderConfiguration
    {
        bool IsTriggered { get; }
        ReminderSlot Slot { get; }
        bool Active { get; }
        bool AutoOff { get; }
        ReminderTrigger Trigger { get; }
        decimal TargetValue { get; }
        int Duration { get; }
        BeepType ReminderType { get; }
        BeepPattern ReminderPattern { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Attributes
{
    public interface IReminderAttributes
    {
        bool IsTriggered { get; }
        bool Active { get; }
        bool AutoOff { get; }
        ReminderTrigger Trigger { get; }
        decimal TargetValue { get; }
        TimeSpan Duration { get; }
        BeepType ReminderBeep { get; }
        BeepPattern ReminderBeepPattern { get; }
    }
}

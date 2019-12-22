using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Data.Entities
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

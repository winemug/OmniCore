using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class ReminderSettings
    {
        public bool IsTriggered { get; }
        public bool Active { get; }
        public bool AutoOff { get; }
        public ReminderTrigger Trigger { get; }
        public decimal TargetValue { get; }
        public TimeSpan Duration { get; }
        public BeepType ReminderBeep { get; }
        public BeepPattern ReminderBeepPattern { get; }
    }
}
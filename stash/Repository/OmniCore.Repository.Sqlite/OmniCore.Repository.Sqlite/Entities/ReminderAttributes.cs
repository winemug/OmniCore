using System;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class ReminderAttributes : IReminderAttributes
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
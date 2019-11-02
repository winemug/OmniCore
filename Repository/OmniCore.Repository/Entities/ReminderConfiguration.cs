using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Repository.Enums;

namespace OmniCore.Repository.Entities
{
    public class ReminderConfiguration : Entity
    {
        public bool IsTriggered { get; }
        public ReminderSlot Slot { get; }
        public bool Active { get; }
        public bool AutoOff { get; }
        public ReminderTrigger Trigger { get; }
        public decimal TargetValue { get; }
        public int Duration { get; }
        public BeepType ReminderType { get; }
        public BeepPattern ReminderPattern { get; }
    }
}

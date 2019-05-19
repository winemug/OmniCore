
using OmniCore.Model.Enums;

namespace OmniCore.Model
{
    public class AlertConfiguration
    {
        public int? alert_index = null;
        public bool activate = false;
        public bool trigger_auto_off = false;
        public int? alert_after_minutes = null;
        public decimal? alert_after_reservoir = null;
        public int alert_duration = 0;
        public BeepType beep_type = BeepType.NoSound;
        public BeepPattern beep_repeat_type = BeepPattern.Once;
    }
}

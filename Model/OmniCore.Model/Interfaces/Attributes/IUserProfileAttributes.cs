using OmniCore.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Attributes
{
    public interface IUserProfileAttributes : IReminderSettingsAttributes
    {
        public string Name { get; set; }
        public TherapyAutomation Automation { get; set;
        
        }
    }
}

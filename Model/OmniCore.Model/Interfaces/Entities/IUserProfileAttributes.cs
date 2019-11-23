using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IUserProfileAttributes : IReminderSettingsAttributes
    {
        string Name { get; set; }
        TherapyAutomation Automation { get; set;
        
        }
    }
}

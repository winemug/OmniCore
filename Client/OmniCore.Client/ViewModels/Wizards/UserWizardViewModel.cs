using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class UserWizardViewModel : DialogViewModel
    {
        public ICommand SelectUserProfileTypeCommand { get; }
        public bool OptionAddLocalUserProfile { get; set; } = true;
        public bool OptionAddRemoteUserProfile { get; set; } = false;
        public UserWizardViewModel(IClient client) : base(client)
        {
            SelectUserProfileTypeCommand = new Command(() =>
            {
                if (OptionAddLocalUserProfile)
                {
                    
                }
            });
        }
    }
}
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class UserWizardViewModel : BaseViewModel
    {
        public UserWizardViewModel(IClient client) : base(client)
        {
        }
    }
}
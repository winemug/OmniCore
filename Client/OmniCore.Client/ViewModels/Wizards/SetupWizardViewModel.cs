using System;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class SetupWizardViewModel : BaseViewModel
    {
        public bool RunInBackground { get; set; }
        public bool CreateUserProfile { get; set; }
        public SetupWizardViewModel(IClient client) : base(client)
        {
        }
    }
}
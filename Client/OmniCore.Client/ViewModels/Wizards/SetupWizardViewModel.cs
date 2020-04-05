using System;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class SetupWizardViewModel : NavigationViewModel
    {
        public SetupWizardViewModel(ICoreClient client) : base(client)
        {
        }

        public override Task<Page> GetNextPage()
        {
            throw new NotImplementedException();
        }
    }
}
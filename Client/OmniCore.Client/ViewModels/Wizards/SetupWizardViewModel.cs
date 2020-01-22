using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.SetupWizard;
using OmniCore.Model.Interfaces.Platform.Common;
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

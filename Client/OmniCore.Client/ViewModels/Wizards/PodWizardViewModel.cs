using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Platform;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class PodWizardViewModel : BaseViewModel
    {
        public ICommand NextPageCommand { get; set; }
        protected override Task OnInitialize()
        {
            NextPageCommand = new Command(async () => await NextPage());
            return Task.CompletedTask;
        }

        private async Task NextPage()
        {

        }

        public PodWizardViewModel(ICoreClient client) : base(client)
        {
        }
    }
}

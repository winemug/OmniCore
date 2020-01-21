using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Interfaces.Platform.Common;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class PodWizardViewModel : BaseViewModel
    {
        public ICommand NextPageCommand { get; private set; }

        public PodWizardViewModel(ICoreClient client) : base(client)
        {
            NextPageCommand = new Command(async () => { await GoToNextPage(); });
        }
        private async Task GoToNextPage()
        {
            Page nextPage = null;

            if (nextPage != null)
            {
                await Shell.Current.Navigation.PushAsync(nextPage);
            }
        }
    }
}

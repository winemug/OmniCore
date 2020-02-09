using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.Platform;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class PodWizardViewModel : BaseViewModel
    {
        public ICommand NextPageCommand { get; private set; }
        public ContentView ActiveView { get; private set; }

        private PodWizardCreationOptions Page1 = new PodWizardCreationOptions();
        private PodWizardExistingPodOptions Page2 = new PodWizardExistingPodOptions();
        private PodWizardAdvancedOptions Page3 = new PodWizardAdvancedOptions();
        private PodWizardPodTypeSelection Page4 = new PodWizardPodTypeSelection();
        private PodWizardIntegrationOptions Page5 = new PodWizardIntegrationOptions();
        private PodWizardMedicationSelection Page6 = new PodWizardMedicationSelection();
        private PodWizardRadioSelection Page7 = new PodWizardRadioSelection();

        public bool OptionActivateNewPod { get; set; }

        public PodWizardViewModel(ICoreClient client) : base(client)
        {
            ActiveView = Page1;
            NextPageCommand = new Command(async () => { await GoToNextPage(); });
        }
        private async Task GoToNextPage()
        {
            //var nextPage = NextPageFunction?.Invoke();
            //if (nextPage != null)
            //{
            //    await Shell.Current.Navigation.PushAsync(nextPage);
            //}
        }
    }
}

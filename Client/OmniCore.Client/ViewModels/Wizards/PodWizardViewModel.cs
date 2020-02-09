using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.Platform;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class PodWizardViewModel : BaseViewModel
    {
        public ICommand NextPageCommand { get; private set; }
        public ContentView ActiveView { get; private set; }

        private PodWizardCreationOptions CreationOptionsView = new PodWizardCreationOptions();
        private PodWizardExistingPodOptions ExistingInActivationView = new PodWizardExistingPodOptions();
        private PodWizardAdvancedOptions Page3 = new PodWizardAdvancedOptions();
        private PodWizardIntegrationOptions Page5 = new PodWizardIntegrationOptions();
        private PodWizardMedicationSelection Page6 = new PodWizardMedicationSelection();
        private PodWizardRadioSelection Page7 = new PodWizardRadioSelection();

        public PodWizardViewModel(ICoreClient client) : base(client)
        {
            NextPageCommand = new Command(async () => { await GoToNextPage(); });
        }

        protected override async Task OnPageAppearing()
        {
            if (Parameter == null)
            {
                var activePods = await Api.CorePodService.ActivePods(CancellationToken.None);
                if (activePods.Any(p => p.RunningState.State <= PodState.Started))
                {
                    ActiveView = ExistingInActivationView;
                }
                else
                {
                    ActiveView = CreationOptionsView;
                }
            }
            else
            {
                //
            }

            await base.OnPageAppearing();
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

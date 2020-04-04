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
        public ICommand StartErosCommand { get; }
        public ICommand StartDashCommand { get; }
        public ICommand RecoverErosCommand { get; }

        public ICommand NextPageCommand { get; private set; }

        public int CarouselPosition { get; set; }

        public ObservableCollection<ContentView> Views { get; private set; }

        private bool SelectedEros;
        private bool SelectedDash;
        private bool SelectedRecovery;

        private PodWizardPodTypeSelection PodTypeSelectionView = new PodWizardPodTypeSelection();
        private PodWizardPendingActivationsWarning PendingActivationsWarningView = new PodWizardPendingActivationsWarning();
        private PodWizardAdvancedOptions Page3 = new PodWizardAdvancedOptions();
        private PodWizardIntegrationOptions Page5 = new PodWizardIntegrationOptions();
        private PodWizardMedicationSelection Page6 = new PodWizardMedicationSelection();
        private PodWizardRadioSelection RadioSelectionView = new PodWizardRadioSelection();

        public PodWizardViewModel(ICoreClient client) : base(client)
        {
            NextPageCommand = new Command(async () => { await GoToNextPage(); });

            StartErosCommand = new Command(() =>
            {
                SelectedEros = true;
                SelectedRecovery = false;
                SelectedDash = false;
                CarouselPosition += 1;
            });

            StartDashCommand  = new Command(() =>
            {
                SelectedEros = false;
                SelectedRecovery = false;
                SelectedDash = true;
            });

            RecoverErosCommand = new Command(() =>
            {
                SelectedEros = true;
                SelectedRecovery = true;
                SelectedDash = false;
            });

        }

        protected override async Task OnPageAppearing()
        {
            Views = new ObservableCollection<ContentView>();
            if (Parameter == null)
            {
                var activePods = await Api.CorePodService.ActivePods(CancellationToken.None);
                if (activePods.Any(p => p.RunningState.State <= PodState.Started))
                {
                    Views.Add(PendingActivationsWarningView);
                }

                Views.Add(PodTypeSelectionView);
                Views.Add(RadioSelectionView);
            }
            await base.OnPageAppearing();
        }

        private Task GoToNextPage()
        {
            return Task.CompletedTask;
            //var nextPage = NextPageFunction?.Invoke();
            //if (nextPage != null)
            //{
            //    await Shell.Current.Navigation.PushAsync(nextPage);
            //}
        }
    }
}

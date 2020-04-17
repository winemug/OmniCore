using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class PodWizardViewModel : BaseViewModel
    {
        public ICommand StartErosCommand => new Command(() =>
        {
            SelectedEros = true;
            SelectedRecovery = false;
            SelectedDash = false;
            CarouselPosition += 1;
        });
        
        public ICommand StartDashCommand => new Command(() =>
        {
            SelectedEros = false;
            SelectedRecovery = false;
            SelectedDash = true;
        });
        
        public ICommand RecoverErosCommand { get; }
        public ICommand NextPageCommand { get; }
        public int CarouselPosition { get; set; }
        public ObservableCollection<ContentView> Views => new ObservableCollection<ContentView>();

        
        private PodWizardAdvancedOptions Page3 = new PodWizardAdvancedOptions();
        private PodWizardIntegrationOptions Page5 = new PodWizardIntegrationOptions();
        private PodWizardMedicationSelection Page6 = new PodWizardMedicationSelection();

        private readonly PodWizardPendingActivationsWarning PendingActivationsWarningView =
            new PodWizardPendingActivationsWarning();

        private readonly PodWizardPodTypeSelection PodTypeSelectionView = new PodWizardPodTypeSelection();
        private readonly PodWizardRadioSelection RadioSelectionView = new PodWizardRadioSelection();
        private bool SelectedDash;
        private bool SelectedEros;
        private bool SelectedRecovery;

        public PodWizardViewModel(IClient client) : base(client)
        {
            WhenPageAppears().Subscribe(async _ =>
            {
                var api = await Client.GetApi(CancellationToken.None);
                var activePods = await api.PodService.ActivePods(CancellationToken.None);
                if (activePods.Any(p => p.RunningState.State <= PodState.Started))
                    Views.Add(PendingActivationsWarningView);

                Views.Add(PodTypeSelectionView);
                Views.Add(RadioSelectionView);
            });
        }
    }
}
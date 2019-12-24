using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Testing;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Testing
{
    public class RadiosViewModel : BaseViewModel
    {
        public ObservableCollection<IRadio> Radios { get; set; }

        public ICommand BlinkCommand { get; set; }
        public ICommand SelectCommand { get; set; }

        private IDisposable ListRadiosSubscription;

        [Unity.Dependency]
        public IUnityContainer Container { get; set; }

        [Unity.Dependency(nameof(RegistrationConstants.RileyLink))]
        public IRadioService RileyLinkRadioService { get; set; }
        [Unity.Dependency]
        public ICoreApplicationServices ApplicationServices { get; set; }

        public RadiosViewModel()
        {
            Title = "Radio Selection";
            BlinkCommand = new Command<IRadio>(async radio => await IdentifyRadio(radio), (radio) => radio != null && !radio.InUse);
            SelectCommand = new Command<IRadio>(async radio => await SelectRadio(radio), (radio) => radio != null && !radio.InUse);
        }

        public override async Task Initialize()
        {
            Radios = new ObservableCollection<IRadio>();
            ListRadiosSubscription = RileyLinkRadioService.ListRadios()
                .ObserveOn(ApplicationServices.UiSynchronizationContext)
                .Subscribe(radio =>
                    {
                        Radios.Add(radio);
                    });
        }

        public override async Task Dispose()
        {
            ListRadiosSubscription?.Dispose();
            ListRadiosSubscription = null;
        }

        private async Task IdentifyRadio(IRadio radio)
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var radioConnection = await radio.Lease(cancellation.Token);
            await radioConnection.Identify(cancellation.Token);
        }

        private async Task SelectRadio(IRadio radio)
        {
            var radioDiagnosticsView = Container.Resolve<RadioDiagnosticsView>();
            radioDiagnosticsView.ViewModel.Radio = radio;
            await Shell.Current.Navigation.PushAsync(radioDiagnosticsView);
        }
    }
}

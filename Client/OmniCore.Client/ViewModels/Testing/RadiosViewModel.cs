using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Testing;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Workflow;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Testing
{
    public class RadiosViewModel : BaseViewModel
    {
        public ObservableCollection<IRadio> Radios { get; set; }

        public ICommand BlinkCommand { get; }
        public ICommand SelectCommand { get; }

        private IDisposable ListRadiosSubscription;

        [Unity.Dependency]
        private IUnityContainer Container;
        [Unity.Dependency(nameof(RegistrationConstants.RileyLink))]
        private IRadioProvider RileyLinkRadioProvider;
        public RadiosViewModel()
        {
            Title = "Radio Selection";
            BlinkCommand = new Command<IRadio>(async radio => await IdentifyRadio(radio));
            SelectCommand = new Command<IRadio>(async radio => await SelectRadio(radio));
        }

        public override async Task Initialize()
        {
            Radios = new ObservableCollection<IRadio>();
            ListRadiosSubscription = RileyLinkRadioProvider.ListRadios().Subscribe(radio =>
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
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var radioConnection = await radio.Lease(cancellation.Token);
            await radioConnection.Identify(cancellation.Token);
        }

        private async Task SelectRadio(IRadio radio)
        {
            var radioDiagnosticsView = Container.Resolve<RadioDiagnosticsView>();
        }
    }
}

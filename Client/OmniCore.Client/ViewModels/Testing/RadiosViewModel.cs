using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Client.ViewModels.Testing
{
    public class RadiosViewModel : BaseViewModel
    {
        public ObservableCollection<IRadio> Radios { get; set; }

        private readonly IRadioProvider RileyLinkRadioProvider;

        private IDisposable ListRadiosSubscription;
        public RadiosViewModel(IRadioProvider[] providers)
        {
            RileyLinkRadioProvider = providers[0];
        }

        public override async Task Initialize()
        {
            Radios = new ObservableCollection<IRadio>();
            ListRadiosSubscription = RileyLinkRadioProvider.ListRadios(CancellationToken.None).Subscribe(radio =>
            {
                Radios.Add(radio);
            });
        }

        public override async Task Dispose()
        {
            ListRadiosSubscription?.Dispose();
            ListRadiosSubscription = null;
        }
    }
}

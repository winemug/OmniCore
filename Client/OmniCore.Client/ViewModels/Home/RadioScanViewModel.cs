using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Client.ViewModels.Home
{
    public class RadioScanViewModel : BaseViewModel
    {
        public ObservableCollection<RadioPeripheralModel> Peripherals;

        public RadioScanViewModel(ICoreClient client) : base(client)
        {
        }

        protected override Task OnPageAppearing()
        {
            Peripherals = new ObservableCollection<RadioPeripheralModel>();
            ServiceApi.RadioService.ScanRadios()
                .ObserveOn(Client.SynchronizationContext)
                .Subscribe(peripheral =>
                {
                    Peripherals.Add(new RadioPeripheralModel(peripheral));
                })
                .AutoDispose(this);
            
            return Task.CompletedTask;
        }
    }
}

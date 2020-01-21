using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Utilities;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class RadioScanViewModel : BaseViewModel
    {
        public ObservableCollection<RadioPeripheralModel> Peripherals { get; set; }

        public ICommand AddCommand { get; set; }

        public RadioScanViewModel(ICoreClient client) : base(client)
        {
            AddCommand = new Command(async (_) => { await AddSelected();});
        }

        protected override Task OnPageAppearing()
        {
            Peripherals = new ObservableCollection<RadioPeripheralModel>();
            Api.RadioService.ScanRadios()
                .ObserveOn(Client.SynchronizationContext)
                .Subscribe(peripheral =>
                {
                    Peripherals.Add(new RadioPeripheralModel(peripheral));
                })
                .AutoDispose(this);
            
            return Task.CompletedTask;
        }

        private async Task AddSelected()
        {
            var selected = Peripherals.Where(p => p.IsChecked).ToList();
            foreach (var peripheral in selected)
            {
                var progress = new TaskProgress();
                var popup = Client.ViewPresenter.GetView<ProgressPopupView>(false, progress);
                await Api.RadioService.VerifyPeripheral(peripheral.Peripheral);
            }
        }
    }
}

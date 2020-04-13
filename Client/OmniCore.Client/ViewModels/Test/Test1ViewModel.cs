using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services.Facade;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Test
{
    public class Test1ViewModel : TaskViewModel
    {
        public Test1ViewModel(ICoreClient client) : base(client)
        {
            IdentifyCommand = new Command(async () => await Identify());
            Radios = new ObservableCollection<RadioModel>();
        }

        public Command IdentifyCommand { get; }

        public ObservableCollection<RadioModel> Radios { get; set; }

        private IDisposable ScanSub = null;
        protected override async Task OnPageAppearing()
        {
            await Task.Run(() =>
            {
                ScanSub?.Dispose();
                ScanSub = Api.PodService.ListErosRadios().Subscribe(
                    radio => { Radios.Add(new RadioModel(radio)); });
            });
        }

        protected override async Task OnPageDisappearing()
        {
            await Task.Run(() =>
            {
                ScanSub?.Dispose();
                ScanSub = null;
            });
        }

        private async Task Identify()
        {
            var user = await Api.ConfigurationService.GetDefaultUser(CancellationToken.None);
            var med = await Api.ConfigurationService.GetDefaultMedication(CancellationToken.None);
            var pod = await Api.PodService.NewErosPod(user, med, CancellationToken.None);

            await pod.UpdateRadioList(Radios
                    .Where(r => r.IsChecked)
                    .Select(r => r.Radio), CancellationToken.None);
        }
    }
}
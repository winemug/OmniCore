using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Test
{
    public class Test1ViewModel : TaskViewModel
    {
        public ObservableCollection<RadioModel> Radios => new ObservableCollection<RadioModel>();
        public Command IdentifyCommand => new Command(async () =>
        {
            var api = await Client.GetServiceApi(CancellationToken.None);
            var user = await api.ConfigurationService.GetDefaultUser(CancellationToken.None);
            var med = await api.ConfigurationService.GetDefaultMedication(CancellationToken.None);
            var pod = await api.PodService.NewErosPod(user, med, CancellationToken.None);

            await pod.UpdateRadioList(Radios
                .Where(r => r.IsChecked)
                .Select(r => r.Radio), CancellationToken.None);
        });

        private IDisposable ScanSub = null;
        public Test1ViewModel(IClient client) : base(client)
        {
            WhenPageAppears().Subscribe(async _ =>
            {
                var api = await Client.GetServiceApi(CancellationToken.None);
                ScanSub?.Dispose();
                ScanSub = api.PodService.ListErosRadios().Subscribe(
                    radio => { Radios.Add(new RadioModel(radio)); });
            });
            WhenPageDisappears().Subscribe(_ =>
            {
                ScanSub?.Dispose();
                ScanSub = null;
            });
        }
    }
}
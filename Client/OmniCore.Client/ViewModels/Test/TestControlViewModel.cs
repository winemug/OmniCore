using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Test
{
    public class TestControlViewModel : TaskViewModel
    {
        public Command StartTestCommand => new Command(async () =>
        {
            if (Radios.Any(r => r.IsChecked))
            {
                StartTestEnabled = false;

                var api = await Client.GetServiceApi(CancellationToken.None);
                var user = await api.ConfigurationService.GetDefaultUser(CancellationToken.None);
                var med = await api.ConfigurationService.GetDefaultMedication(CancellationToken.None);
                var pod = await api.PodService.NewErosPod(user, med, CancellationToken.None);

                await pod.UpdateRadioList(Radios
                    .Where(r => r.IsChecked)
                    .Select(r => r.Radio), CancellationToken.None);

                StopScan();
                StopTestEnabled = true;
            }
        });

        public Command StopTestCommand => new Command(async () =>
        {
            StopTestEnabled = false;
            var api = await Client.GetServiceApi(CancellationToken.None);
            var activePods = await api.PodService.ActivePods(CancellationToken.None);
            foreach (var activePod in activePods)
                await activePod.Archive(CancellationToken.None);

            StartTestEnabled = true;
            await StartScan();
        });

        public bool StartTestEnabled { get; set; }
        public bool StopTestEnabled { get; set; }
        public ObservableCollection<RadioModel> Radios { get; }

        private IDisposable ScanSub = null;
        public TestControlViewModel(IClient client) : base(client)
        {
            Radios = new ObservableCollection<RadioModel>();

            WhenPageAppears().Subscribe(async _ =>
            {
                var api = await Client.GetServiceApi(CancellationToken.None);

                var activePods = await api.PodService.ActivePods(CancellationToken.None);
                if (activePods.Any())
                {
                    StartTestEnabled = false;
                    StopTestEnabled = true;
                }
                else
                {
                    StartTestEnabled = true;
                    StopTestEnabled = false;
                    await StartScan();
                }

            }).DisposeWith(this);
            
            WhenPageDisappears().Subscribe(_ =>
            {
                StopScan();
            }).DisposeWith(this);
        }

        private async Task StartScan()
        {
            StopScan();
            var api = await Client.GetServiceApi(CancellationToken.None);
            ScanSub = api.PodService.ListErosRadios().Subscribe(
                radio => { Radios.Add(new RadioModel(radio)); });
        }

        private void StopScan()
        {
            ScanSub?.Dispose();
            ScanSub = null;
            Radios.Clear();
        }
    }
}
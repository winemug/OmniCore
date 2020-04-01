using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services.Facade;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Test
{
    public class Test1ViewModel : TaskViewModel
    {
        public Command AcquireCommand { get; }

        public ObservableCollection<RadioModel> Radios { get; set; }
        
        public Test1ViewModel(ICoreClient client) : base(client)
        {
            AcquireCommand = new Command(async () => await Acquire());
            Radios = new ObservableCollection<RadioModel>();
        }

        protected override Task OnPageAppearing()
        {
            Disposables.Add(Api.CorePodService.ListErosRadios().Subscribe(
                radio => { Radios.Add(new RadioModel(radio)); }));
            return base.OnPageAppearing();
        }

        private async Task Acquire()
        {
            DisposeDisposables();

            var user = await Api.CoreConfigurationService.GetDefaultUser();
            var med = await Api.CoreConfigurationService.GetDefaultMedication();
            var pod = await Api.CorePodService.NewErosPod(user, med, CancellationToken.None);
            var selection = Radios.FirstOrDefault(r => r.IsChecked);
            if (selection != null)
            {
                var request = await pod.Acquire(selection.Radio, CancellationToken.None);
                await request.ExecuteRequest();
                this.SetTask(request);
            }
        }
    }
}

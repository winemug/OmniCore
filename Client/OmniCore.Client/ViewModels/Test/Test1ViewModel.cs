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
        public Command IdentifyCommand { get; }

        public ObservableCollection<RadioModel> Radios { get; set; }
        
        public Test1ViewModel(ICoreClient client) : base(client)
        {
            IdentifyCommand = new Command(async () => await Identify());
            Radios = new ObservableCollection<RadioModel>();
        }

        protected override Task OnPageAppearing()
        {
            Disposables.Add(Api.CorePodService.ListErosRadios().Subscribe(
                radio => { Radios.Add(new RadioModel(radio)); }));
            return base.OnPageAppearing();
        }

        private async Task Identify()
        {
            DisposeDisposables();
            foreach (var radio in Radios.Where(r => r.IsChecked))
                await radio.Radio.Identify(CancellationToken.None);

        }
    }
}

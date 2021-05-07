﻿using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class RadiosViewModel : BaseViewModel
    {
        public ObservableCollection<RadioModel> Radios { get; } = new ObservableCollection<RadioModel>();
        public ICommand SelectCommand => new Command<RadioModel>(async rm => 
            await Client.NavigateTo<RadioDetailView>(rm));
        public ICommand AddCommand => new Command(async _ =>
        {
            await Client.NavigateTo<RadioScanView>();
        });
        
        public RadiosViewModel(IClient client) : base(client)
        {
            WhenPageAppears().Subscribe(async _ =>
            {
                var api = await client.GetServiceApi(CancellationToken.None);
                api.PodService.ListErosRadios()
                    .ObserveOn(await Device.GetMainThreadSynchronizationContextAsync())
                    .Subscribe(radio => { Radios.Add(new RadioModel(radio)); });
            });
        }
    }
}
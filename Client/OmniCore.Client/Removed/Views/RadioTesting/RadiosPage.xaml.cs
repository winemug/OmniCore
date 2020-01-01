using Microsoft.AppCenter.Crashes;
using OmniCore.Client.ViewModels.Test;
using OmniCore.Model.Interfaces;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Model.Interfaces.Workflow;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.RadioTesting
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadiosPage : ContentPage
    {
        private RadioTestingViewModel ViewModel;

        public RadiosPage()
        {
            InitializeComponent();
        }

        public RadiosPage WithViewModel(RadioTestingViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.NextPageCommand = new Command(async (o) =>
            {
                viewModel.Radio = o as IRadio;
                var page = new PodsPage().WithViewModel(viewModel);
                await Navigation.PushAsync(page);
            },
            (_) => true);
            BindingContext = ViewModel;
            return this;
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            await ViewModel.StartScanning();
        }

        private async void ContentPage_Disappearing(object sender, EventArgs e)
        {
            await ViewModel.StopScanning();
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Interfaces;
using OmniCore.Client.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OmniCore.Client.ViewModels.Test;
using OmniCore.Repository.Entities;
using System.Collections.ObjectModel;

namespace OmniCore.Client.Views.RadioTesting
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodsPage : ContentPage
    {
        private RadioTestingViewModel ViewModel;

        public PodsPage()
        {
            InitializeComponent();
        }

        public PodsPage WithViewModel(RadioTestingViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.NextPageCommand = new Command(async (o) =>
            {
                viewModel.Pod = o as Pod;
                var page = new RadioTestPage().WithViewModel(viewModel);
                await Navigation.PushAsync(page);
            },
            (_) => true);
            BindingContext = ViewModel;
            return this;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var idString = await this.DisplayPromptAsync("Register Pod", "Enter pod radio id, e.g.: 0x34123456 or 873608278");
            uint podRadioId;
            if (uint.TryParse(idString, out podRadioId))
            {
                await ViewModel.AddPod(podRadioId);
            }
            else
            {
                await this.DisplayAlert("Register Pod", "Invalid pod radio id.", "OK");
            }
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            await ViewModel.LoadPods();
        }
    }
}
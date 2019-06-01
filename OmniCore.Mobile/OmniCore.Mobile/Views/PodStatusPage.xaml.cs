using OmniCore.Mobile.ViewModels;
using OmniCore.Model;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodStatusPage : ContentPage
    {
        PodStatusViewModel viewModel;

        public PodStatusPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new PodStatusViewModel();
        }

        private async void Update_Button_Clicked(object sender, EventArgs e)
        {
            viewModel.UpdateButtonEnabled = false;
            try
            {
                var cts = new CancellationTokenSource();
                var progress = new MessageProgress();
                await Task.Run(async () => await App.PodProvider.Current.UpdateStatus(progress, cts.Token).ConfigureAwait(false));
            }
            finally
            {
                viewModel.UpdateButtonEnabled = true;
            }
        }
    }
}
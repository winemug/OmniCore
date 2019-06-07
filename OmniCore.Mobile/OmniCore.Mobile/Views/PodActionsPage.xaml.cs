using OmniCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodActionsPage : ContentPage
    {
        public PodActionsPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var progress = new MessageExchangeProgress();
            MeView.SetProgress(progress);
            await Task.Run(async () => await App.PodProvider.Current.Bolus(progress, 0.15m).ConfigureAwait(false));
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            var progress = new MessageExchangeProgress();
            MeView.SetProgress(progress);
            await Task.Run(async () => await App.PodProvider.Current.SetTempBasal(progress, 1.5m, 0.5m).ConfigureAwait(false));
        }
    }
}
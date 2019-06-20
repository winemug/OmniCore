using OmniCore.Mobile.ViewModels.Pod;
using OmniCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views.Pod
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Fody.ConfigureAwait(true)]
    public partial class DebugPage : ContentPage
    {
        private DebugViewModel viewModel;

        public DebugPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new DebugViewModel();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var podManager = App.Instance.PodProvider.PodManager;
            using(var conversation = await podManager.StartConversation())
            {
                await podManager.Bolus(conversation, 1.0m);
            }
            
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            var podManager = App.Instance.PodProvider.PodManager;
            using (var conversation = await podManager.StartConversation())
            {
                await podManager.SetTempBasal(conversation, 30m, 0.5m);
            }
        }

        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            var podManager = App.Instance.PodProvider.PodManager;
            using (var conversation = await podManager.StartConversation())
            {
                await podManager.CancelTempBasal(conversation);
            }
        }
    }
}
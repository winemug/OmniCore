using OmniCore.Mobile.ViewModels;
using OmniCore.Model.Interfaces;
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
    public partial class MessageExchangeView : ContentView
    {
        MessageExchangeViewModel ViewModel;
        public MessageExchangeView()
        {
            InitializeComponent();
            BindingContext = ViewModel = new MessageExchangeViewModel();
        }

        public void SetProgress(IMessageExchangeProgress progress)
        {
            this.ViewModel.ExchangeProgress = progress;
        }
    }
}
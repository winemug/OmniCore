using OmniCore.Mobile.ViewModels.Pod;
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
    public partial class ConversationPage : ContentPage
    {
        private ConversationViewModel viewModel;
        public ConversationPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new ConversationViewModel();
        }
    }
}
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
    public partial class ConversationsPage : ContentPage
    {
        private ConversationsViewModel viewModel;
        public ConversationsPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new ConversationsViewModel();
        }
    }
}
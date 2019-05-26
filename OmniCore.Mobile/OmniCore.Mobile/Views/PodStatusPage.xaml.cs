using OmniCore.Mobile.ViewModels;
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
    public partial class PodStatusPage : ContentPage
    {
        PodStatusViewModel viewModel;

        public PodStatusPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new PodStatusViewModel();
        }
    }
}
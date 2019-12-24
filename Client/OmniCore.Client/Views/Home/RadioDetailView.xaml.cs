using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Home;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadioDetailView
    {
        public RadioDetailView(RadioDetailViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Testing;
using OmniCore.Client.Views.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Testing
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadiosView : BaseView<RadiosViewModel>
    {
        public RadiosView(RadiosViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.Views.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadiosView : BaseView<RadiosViewModel>
    {
        public RadiosView(RadiosViewModel viewModel, UnityRouteFactory routeFactory) : base(viewModel)
        {
            Routing.RegisterRoute("Home//Radios//Detail", routeFactory.WithType(typeof(RadioDetailView)));
            InitializeComponent();
        }
    }
}
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Main;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Platform.Client;
using Xamarin.Forms;

namespace OmniCore.Client
{
    public partial class XamarinApp : Application, IClientResolvable
    {
        public XamarinApp(IViewPresenter viewPresenter)
        {
            InitializeComponent();
            RegisterViews(viewPresenter);
            MainPage = new ShellView(viewPresenter);
        }

        private void RegisterViews(IViewPresenter viewPresenter)
        {
            viewPresenter
                .WithViewViewModel<PodsView, PodsViewModel>()
                .WithViewViewModel<RadiosView, RadiosViewModel>()
                .WithViewViewModel<RadioDetailView, RadioDetailViewModel>()
                .WithViewViewModel<RadioScanView, RadioScanViewModel>()
                .WithViewViewModel<EmptyView, EmptyViewModel>();
        }
    }
}

using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.ViewModels.Wizards;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
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
                .WithViewViewModel<RadioScanView, RadiosViewModel>()
                .WithViewViewModel<ProgressPopupView, ProgressPopupViewModel>()
                .WithViewViewModel<EmptyView, EmptyViewModel>()
                .WithViewViewModel<PodWizardMainView, PodWizardViewModel>()
                .WithViewViewModel<NavigationView, NavigationViewModel>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Acr.Logging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.ViewModels.Test;
using OmniCore.Client.ViewModels.Wizards;
using OmniCore.Client.Views;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Test;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using Xamarin.Forms;

namespace OmniCore.Client
{
    public partial class XamarinClient : ICoreClient
    {
        private readonly ICoreContainer<IClientResolvable> Container;
        private readonly ICorePlatformClient PlatformClient;

        private readonly Dictionary<Type, Func<bool, object, IView>> ViewDictionary;
        private readonly ICoreClientConnection ApiConnection;
        private readonly ICoreLoggingFunctions Logging;
        private readonly NavigationPage MainNavigation;
        
        public XamarinClient(
            ICoreContainer<IClientResolvable> container,
            ICoreClientConnection apiConnection,
            ICoreLoggingFunctions logging)
        {
            Logging = logging;
            Container = container;
            ApiConnection = apiConnection;
            ViewDictionary = new Dictionary<Type, Func<bool, object, IView>>();
            RegisterViews();
            InitializeComponent();
            MainPage = new NavigationPage(GetView<SplashView>(false));

            ApiConnection.WhenConnected().Subscribe(async api =>
            {
                Logging.Debug("Service connected.");
                await MainNavigation.PushAsync(GetView<ShellView>(false), true);
            }, async e =>
            {
                Logging.Error("Service connection failed.", e);
                await MainNavigation.PopAsync(true);
            });
            
            ApiConnection.WhenDisconnected().Subscribe(async _ =>
            {
                Logging.Debug("Service disconnected.");
                await ApiConnection.Connect();
            });
        }
      
        public T GetView<T>(bool viaShell, object parameter = null)
            where T : IView
        {
            return (T) ViewDictionary[typeof(T)](viaShell, parameter);
        }

        public Task<ICoreApi> GetApi(CancellationToken cancellationToken) => 
            ApiConnection.WhenConnected().ToTask(cancellationToken);


        public Task PushView<T>() where T : IView
        {
            return PushView(GetView<T>(false));
        }
        
        public Task PushView<T>(object parameter) where T : IView
        {
            return PushView(GetView<T>(false, parameter));
        }

        private async Task PushView(IView view)
        {
            await MainNavigation.PushAsync((Page) view);
        }

        private void RegisterViews()
        {
            RegisterViewViewModel<SplashView, SplashViewModel>();
            RegisterViewViewModel<ShellView, ShellViewModel>();
            RegisterViewViewModel<ServicePopupView, ServicePopupViewModel>();
            RegisterViewViewModel<ActivePodsView, ActivePodsViewModel>();
            RegisterViewViewModel<RadiosView, RadiosViewModel>();
            RegisterViewViewModel<RadioDetailView, RadioDetailViewModel>();
            RegisterViewViewModel<RadioScanView, RadiosViewModel>();
            RegisterViewViewModel<ProgressPopupView, ProgressPopupViewModel>();
            RegisterViewViewModel<EmptyView, EmptyViewModel>();
            RegisterViewViewModel<PodWizardMainView, PodWizardViewModel>();
            RegisterViewViewModel<Test1View, Test1ViewModel>();
        }
        
        private void RegisterViewViewModel<TView, TViewModel>()
            where TView : IView
            where TViewModel : IViewModel
        {
            Container.One<TView>();
            Container.One<TViewModel>();

            ViewDictionary.Add(typeof(TView), (viaShell, parameter) =>
            {
                var view = Container.Get<TView>();
                var viewModel = Container.Get<TViewModel>();
                viewModel.Initialize(view, viaShell, parameter);
                return view;
            });
        }
    }
}
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
using OmniCore.Client.Views.Wizards.SetupWizard;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using Xamarin.Forms;

namespace OmniCore.Client
{
    public partial class XamarinClient : IClient, IInitializable
    {
        private readonly IContainer<IClientInstance> Container;
        private readonly IClientFunctions ClientFunctions;
        private readonly ICommonFunctions CommonFunctions;

        private readonly Dictionary<Type, Func<bool, object, IView>> ViewDictionary;
        private readonly IClientConnection ApiConnection;
        private readonly ILogger Logger;
        private readonly NavigationPage MainNavigation;
        
        public XamarinClient(
            IContainer<IClientInstance> container,
            IClientConnection apiConnection,
            ILogger logger,
            ICommonFunctions commonFunctions)
        {
            CommonFunctions = commonFunctions;
            Logger = logger;
            Container = container;
            ApiConnection = apiConnection;
            ViewDictionary = new Dictionary<Type, Func<bool, object, IView>>();
            RegisterViews();
            InitializeComponent();
        }

        public async Task Initialize()
        {
            MainPage = new NavigationPage(GetView<SplashView>(false));

            var context = await Device.GetMainThreadSynchronizationContextAsync();
            ApiConnection.WhenConnected().SubscribeOn(context)
                .Subscribe(async api =>
            {
                Logger.Debug("Service connected.");
                await MainNavigation.PushAsync(GetView<ShellView>(false), true);
            }, async e =>
            {
                Logger.Error("Service connection failed.", e);
                await MainNavigation.PopAsync(true);
            });
            
            ApiConnection.WhenDisconnected().SubscribeOn(context)
                .Subscribe(async _ =>
            {
                Logger.Debug("Service disconnected.");
            });

            await ApiConnection.Connect();
        }
        public T GetView<T>(bool viaShell, object parameter = null)
            where T : IView
        {
            return (T) ViewDictionary[typeof(T)](viaShell, parameter);
        }

        public Task<IApi> GetApi(CancellationToken cancellationToken) => 
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
            RegisterViewViewModel<SetupWizardRootView, SetupWizardViewModel>();
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
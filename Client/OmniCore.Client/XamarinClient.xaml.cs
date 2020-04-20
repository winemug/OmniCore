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
using OmniCore.Client.Views.Dialogs;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Test;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Client.Views.Wizards.NewUser;
using OmniCore.Client.Views.Wizards.Permissions;
using OmniCore.Client.Views.Wizards.SetupWizard;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client
{
    public partial class XamarinClient : IClient, IInitializable
    {
        private readonly IContainer<IClientInstance> Container;
        private readonly IClientFunctions ClientFunctions;
        private readonly ICommonFunctions CommonFunctions;
        private readonly IPlatformConfiguration PlatformConfiguration;

        private readonly Dictionary<Type, Func<bool, object, Task<IView>>> ViewDictionary;
        private readonly IClientConnection ApiConnection;
        private readonly ILogger Logger;

        private NavigationPage MainNavigation;
        
        public XamarinClient(
            IContainer<IClientInstance> container,
            IClientConnection apiConnection,
            ILogger logger,
            ICommonFunctions commonFunctions,
            IPlatformConfiguration platformConfiguration)
        {
            CommonFunctions = commonFunctions;
            PlatformConfiguration = platformConfiguration;
            Logger = logger;
            Container = container;
            ApiConnection = apiConnection;
            ViewDictionary = new Dictionary<Type, Func<bool, object, Task<IView>>>();
            RegisterViews();
            InitializeComponent();
        }

        public async Task Initialize()
        {
            var context = await Device.GetMainThreadSynchronizationContextAsync();
            ApiConnection.WhenConnected().SubscribeOn(context)
                .Subscribe(async api =>
            {
                Logger.Debug("Service connected.");
                await MainNavigation.PushAsync(await GetView<ShellView>(false), true);
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

            MainNavigation = new NavigationPage(await GetView<SplashView>(false));
            MainPage = MainNavigation;

            while (!PlatformConfiguration.TermsAccepted)
            {
                if (!await ShowDialog<TermsDialogView>(CancellationToken.None))
                {
                    CommonFunctions.Exit();
                }
            }

            while (!await ClientFunctions.BluetoothPermissionGranted() ||
                   !await ClientFunctions.StoragePermissionGranted())
            {
                if (!await ShowDialog<PermissionsDialogView>(CancellationToken.None))
                {
                    CommonFunctions.Exit();
                }
            }

            while (!PlatformConfiguration.DefaultUserSetUp)
            {
                if (!await ShowDialog<UserWizardRootView>(CancellationToken.None))
                {
                    CommonFunctions.Exit();
                }
            }
            
            await ApiConnection.Connect();
        }
        public Task<IApi> GetApi(CancellationToken cancellationToken) => 
            ApiConnection.WhenConnected().ToTask(cancellationToken);

        public async Task PushView<T>() where T : IView
        {
            await PushView(await GetView<T>(false));
        }
        
        public async Task PushView<T>(object parameter) where T : IView
        {
            await PushView(await GetView<T>(false, parameter));
        }

        private async Task<bool> ShowDialog<T>(CancellationToken cancellationToken) where T : IView
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
            });
            
            var confirm = new Func<Task>(() =>
            {
                tcs.TrySetResult(true);
                return Task.CompletedTask;
            });
            
            var cancel = new Func<Task>(() =>
            {
                tcs.TrySetResult(false);
                return Task.CompletedTask;
            });
            
            var view = await GetView<T>(false, (confirm, cancel));
            await MainNavigation.PushAsync(view as Page);
            try
            {
                return await tcs.Task.ConfigureAwait(true);
            }
            finally
            {
                await MainNavigation.PopAsync();
            }
        }
        private async Task PushView(IView view)
        {
            await MainNavigation.PushAsync((Page) view);
        }

        private void RegisterViews()
        {
            // main views
            RegisterViewViewModel<EmptyView, EmptyViewModel>();
            RegisterViewViewModel<SplashView, SplashViewModel>();
            RegisterViewViewModel<ShellView, ShellViewModel>();
            RegisterViewViewModel<ServicePopupView, ServicePopupViewModel>();
            
            // home
            RegisterViewViewModel<ActivePodsView, ActivePodsViewModel>();
            RegisterViewViewModel<RadiosView, RadiosViewModel>();
            RegisterViewViewModel<RadioDetailView, RadioDetailViewModel>();
            RegisterViewViewModel<RadioScanView, RadiosViewModel>();
            RegisterViewViewModel<ProgressPopupView, ProgressPopupViewModel>();

            // wizards (of oz)
            RegisterViewViewModel<SetupWizardRootView, SetupWizardViewModel>();
            RegisterViewViewModel<PermissionsWizardRootView, PermissionsWizardViewModel>();
            RegisterViewViewModel<UserWizardRootView, UserWizardViewModel>();
            RegisterViewViewModel<PodWizardMainView, PodWizardViewModel>();
            
            // test views
            RegisterViewViewModel<Test1View, Test1ViewModel>();
        }

        public async Task<T> GetView<T>(bool viaShell, object parameter = null)
            where T : IView
        {
            return (T) await ViewDictionary[typeof(T)](viaShell, parameter);
        }

        private void RegisterViewViewModel<TView, TViewModel>()
            where TView : IView
            where TViewModel : IViewModel
        {
            Container.One<TView>();
            Container.One<TViewModel>();

            ViewDictionary.Add(typeof(TView), async (viaShell, parameter) =>
            {
                var view = await Container.Get<TView>();
                var viewModel = await Container.Get<TViewModel>();
                viewModel.Initialize(view, viaShell, parameter);
                return view;
            });
        }
    }
}
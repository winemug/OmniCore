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
using OmniCore.Client.ViewModels.Base.Dialogs;
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
using OmniCore.Client.Views.Wizards.User;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;


namespace OmniCore.Client
{
    public partial class XamarinClient : IClient, IInitializable
    {
        private readonly IContainer Container;
        private readonly IPlatformFunctions PlatformFunctions;
        private readonly IUserActivity UserActivity;
        private readonly IPlatformConfiguration PlatformConfiguration;
        private readonly IServiceApi ServiceApi;

        private readonly Dictionary<Type, Func<bool, object, Task<IView>>> ViewDictionary;
        private readonly ILogger Logger;
        
        public XamarinClient(
            IContainer container,
            ILogger logger,
            IPlatformFunctions platformFunctions,
            IUserActivity userActivity,
            IServiceApi serviceApi,
            IPlatformConfiguration platformConfiguration)
        {
            Container = container;
            Logger = logger;
            PlatformFunctions = platformFunctions;
            UserActivity = userActivity;
            ServiceApi = serviceApi;
            PlatformConfiguration = platformConfiguration;
            ViewDictionary = new Dictionary<Type, Func<bool, object, Task<IView>>>();
            RegisterViews();
            InitializeComponent();
        }

        public async Task Initialize()
        {
            MainPage = await GetView<ShellView>(false);
        }
        public Task<IServiceApi> GetServiceApi(CancellationToken cancellationToken)
        {
            return ServiceApi
                .ApiStatus.FirstAsync(s => s == CoreApiStatus.Started)
                .Select(_ => ServiceApi)
                .ToTask(cancellationToken);
        }

        public async Task NavigateTo<T>() where T : IView
        {
            throw new NotImplementedException();
        }

        public async Task NavigateTo<T>(object parameter) where T : IView
        {
            throw new NotImplementedException();
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

            await PopupNavigation.Instance.PushAsync(view as PopupPage, true);
            try
            {
                return await tcs.Task;
            }
            finally
            {
                await PopupNavigation.Instance.PopAsync(true);
            }
        }

        private void RegisterViews()
        {
            // main views
            RegisterViewViewModel<EmptyView, EmptyViewModel>();
            RegisterViewViewModel<SplashView, SplashViewModel>();
            RegisterViewViewModel<ShellView, ShellViewModel>();
            
            // home
            RegisterViewViewModel<ActivePodsView, ActivePodsViewModel>();
            RegisterViewViewModel<RadiosView, RadiosViewModel>();
            RegisterViewViewModel<RadioDetailView, RadioDetailViewModel>();
            RegisterViewViewModel<RadioScanView, RadiosViewModel>();
            RegisterViewViewModel<ProgressPopupView, ProgressPopupViewModel>();

            // setup stuff
            RegisterViewViewModel<TermsDialogView, TermsDialogViewModel>();
            RegisterViewViewModel<PermissionsDialogView, PermissionsDialogViewModel>();
            
            // wizards (of oz)
            RegisterViewViewModel<UserWizardRootView, UserWizardViewModel>();
            RegisterViewViewModel<UserWizardLocalUserView, UserWizardViewModel>();
            
            RegisterViewViewModel<PodWizardMainView, PodWizardViewModel>();
            
            // test views
            RegisterViewViewModel<TestControlView, TestControlViewModel>();
            RegisterViewViewModel<TestDetailView, TestDetailViewModel>();
            RegisterViewViewModel<TestLogView, TestLogViewModel>();
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

        protected override async void OnStart()
        {
            base.OnStart();
            while (!PlatformConfiguration.TermsAccepted)
            {
                if (!await ShowDialog<TermsDialogView>(CancellationToken.None))
                {
                    PlatformFunctions.Exit();
                }
            }
            
            while (!await UserActivity.BluetoothPermissionGranted() ||
                   !await UserActivity.StoragePermissionGranted())
            {
                if (!await ShowDialog<PermissionsDialogView>(CancellationToken.None))
                {
                    PlatformFunctions.Exit();
                }
            }

            ServiceApi.ApiStatus.Subscribe(async status =>
            {
                switch (status)
                {
                    case CoreApiStatus.NotStarted:
                        await ServiceApi.StartServices(CancellationToken.None);
                        break;
                    case CoreApiStatus.Starting:
                        await PopupNavigation.Instance.PushAsync(new ServicePopup(), true);
                        break;
                    case CoreApiStatus.Started:
                        await PopupNavigation.Instance.PopAllAsync(true);
                        break;
                    case CoreApiStatus.Failed:
                        break;
                    case CoreApiStatus.Stopping:
                        break;
                    case CoreApiStatus.Stopped:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(status), status, null);
                }
            });


            //TODO: user wizard
            // while (!PlatformConfiguration.DefaultUserSetUp)
            // {
            //     if (!await ShowDialog<UserWizardRootView>(CancellationToken.None))
            //     {
            //         PlatformFunctions.Exit();
            //     }
            // }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void CleanUp()
        {
            base.CleanUp();
        }
    }
}
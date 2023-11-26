using OmniCore.Client.Services;
using OmniCore.Client.Views;

namespace OmniCore.Client
{
    public partial class App : Application
    {
        private readonly NavigationService navigationService;
        private readonly AppService appService;

        public App(NavigationService navigationService,
            AppService appService)
        {
            InitializeComponent();

            MainPage = navigationService.NavigationPage;
            this.navigationService = navigationService;
            this.appService = appService;
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Created += async (_, _) => { await appService.OnWindowCreatedAsync(); };
            window.Activated += async (_, _) => { await appService.OnWindowActivatedAsync(); };
            window.Deactivated += async (_, _) => { await appService.OnWindowDeactivatedAsync(); };
            window.Stopped += async (_, _) => { await appService.OnStoppedAsync(); };
            window.Resumed += async (_, _) => { await appService.OnResumedAsync(); };
            window.Destroying += async (_, _) => { await appService.OnDestroyingAsync(); };
            return window;
        }
    }
}

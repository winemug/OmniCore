using OmniCore.Client.Services;
using OmniCore.Client.Views;

namespace OmniCore.Client
{
    public partial class App : Application
    {
        private readonly NavigationService navigationService;
        private readonly AppEventsService appEventsService;

        public App(NavigationService navigationService,
            AppEventsService appEventsService)
        {
            InitializeComponent();

            MainPage = navigationService.NavigationPage;
            this.navigationService = navigationService;
            this.appEventsService = appEventsService;
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Created += async (_, _) => { await appEventsService.OnWindowCreatedAsync(); };
            window.Activated += async (_, _) => { await appEventsService.OnWindowActivatedAsync(); };
            window.Deactivated += async (_, _) => { await appEventsService.OnWindowDeactivatedAsync(); };
            window.Stopped += async (_, _) => { await appEventsService.OnAppStoppedAsync(); };
            window.Resumed += async (_, _) => { await appEventsService.OnAppResumedAsync(); };
            window.Destroying += async (_, _) => { await appEventsService.OnAppDestroyingAsync(); };
            return window;
        }
    }
}

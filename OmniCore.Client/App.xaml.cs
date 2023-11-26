using OmniCore.Client.Services;
using OmniCore.Client.Views;

namespace OmniCore.Client
{
    public partial class App : Application
    {
        private readonly NavigationService navigationService;

        public App(NavigationService navigationService)
        {
            InitializeComponent();

            MainPage = navigationService.NavigationPage;
            this.navigationService = navigationService;
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            //window.Created += async (_, _) => { await navigationService.OnCreatedAsync(); };
            window.Activated += async (_, _) => { await navigationService.AppWindowActivated(); };
            window.Deactivated += async (_, _) => { await navigationService.AppWindowDeactivated(); };
            //window.Stopped += async (_, _) => { await navigationService.OnStoppedAsync(); };
            //window.Resumed += async (_, _) => { await navigationService.OnResumedAsync(); };
            //window.Destroying += async (_, _) => { await navigationService.OnDestroyingAsync(); };
            return window;
        }

        protected override async void OnStart()
        {
            await navigationService.PushViewAsync<EmptyPage>();
        }
    }
}

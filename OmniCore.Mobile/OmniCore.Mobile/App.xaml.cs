using OmniCore.Mobile.Interfaces;
using OmniCore.Mobile.Services;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Radio.RileyLink;
using Plugin.Permissions;
using System;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;

        public IPodProvider PodProvider { get; private set; }
        public RemoteRequestHandler RequestHandler { get; private set; }

        public App()
        {
            InitializeComponent();
            PodProvider =  new ErosPodProvider(new RileyLinkMessageExchangeProvider(SynchronizationContext.Current));
            RequestHandler = new RemoteRequestHandler();
            DependencyService
                .Get<IRemoteRequestPublisher>(DependencyFetchTarget.GlobalInstance)
                .Subscribe(RequestHandler);

            if (PodProvider.Current == null)
                PodProvider.Register(44538, 1180299, 0x34FF1D55);

            MainPage = new Views.OmniCoreMain();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

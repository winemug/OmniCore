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
        public LocalRequestHandler LocalRequestHandler { get; private set; }

        public App()
        {
            InitializeComponent();
            PodProvider =  new ErosPodProvider(new RileyLinkMessageExchangeProvider(SynchronizationContext.Current));
            LocalRequestHandler = new LocalRequestHandler();
            DependencyService.Get<ILocalRequestPublisher>().Subscribe(LocalRequestHandler);

            // PodProvider.Register(44538, 1140293, 0x34FF1D52);

            MainPage = new Views.OmniCoreMain();
        }

        protected override async void OnStart()
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

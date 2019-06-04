using OmniCore.Mobile.Interfaces;
using OmniCore.Mobile.Services;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Radio.RileyLink;
using System;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile
{
    public partial class App : Application
    {
        public static App Instance => Application.Current as App;
        public static IPodProvider PodProvider;

        public LocalRequestHandler LocalRequestHandler { get; set; }
        public App()
        {
            InitializeComponent();
            PodProvider =  new ErosPodProvider(new RileyLinkProvider(SynchronizationContext.Current));

            LocalRequestHandler = new LocalRequestHandler();
            var publisher = DependencyService.Get<ILocalRequestPublisher>();
            publisher.Subscribe(LocalRequestHandler);

            MainPage = new Views.OmniCoreMain();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
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

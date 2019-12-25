using System.Threading;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client
{
    public partial class XamarinApp : Application, IUserInterface
    {
        public SynchronizationContext SynchronizationContext { get; }
        public Task ShutDown()
        {
            throw new NotImplementedException();
        }

        public IObservable<IUserInterface> WhenStarting()
        {
            return SubjectStarting.AsObservable();
        }

        public IObservable<IUserInterface> WhenHibernating()
        {
            return SubjectHibernating.AsObservable();
        }

        public IObservable<IUserInterface> WhenResuming()
        {
            return SubjectResuming.AsObservable();
        }

        private readonly Subject<IUserInterface> SubjectStarting;
        private readonly Subject<IUserInterface> SubjectHibernating;
        private readonly Subject<IUserInterface> SubjectResuming;
        
        private readonly ICoreServices CoreServices;
        private ICoreApplicationLogger ApplicationLogger => CoreServices.ApplicationServices.ApplicationLogger;
            
        public XamarinApp(ICoreServicesProvider coreServicesProvider, IUnityContainer container)
        {
            SubjectStarting = new Subject<IUserInterface>();
            SubjectHibernating = new Subject<IUserInterface>();
            SubjectResuming = new Subject<IUserInterface>();
            
            CoreServices = coreServicesProvider.LocalServices;
            SynchronizationContext = SynchronizationContext.Current;

            InitializeComponent();

            MainPage = GetMainPage(container);
            ApplicationLogger.Information("OmniCore App initialized");
        }

        protected override async void OnStart()
        {
            SubjectStarting.OnNext(this);
            SubjectStarting.OnCompleted();
        }

        private Page GetMainPage(IUnityContainer container)
        {
            return container.Resolve<ShellView>();
        }

    }
}

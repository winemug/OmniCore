using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository.Sqlite;
using OmniCore.Services;

namespace OmniCore.Client.Droid.Platform
{
    public class CoreServices : ICoreServices
    {
        public ICoreContainer Container { get; private set; }
        public ICoreLoggingService LoggingService { get; private set; }
        public ICoreApplicationService ApplicationService { get; private set; }
        public IRepositoryService RepositoryService { get; private set; }
        public IRadioService RadioService { get; private set; }
        public IPodService PodService { get; private set; }
        public ICoreIntegrationService IntegrationService { get; private set; }

        private readonly ISubject<ICoreServices> UnexpectedStopRequestSubject;

        public IObservable<ICoreServices> OnUnexpectedStopRequest
            => UnexpectedStopRequestSubject.AsObservable();
        
        public CoreServices()
        {
            Container = new OmniCoreContainer()
                .Existing<ICoreServices>(this)
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithAapsIntegrationService()
#if EMULATOR
                .WithBleSimulator()
#else
                .WithCrossBleRadioAdapter()
#endif
                .WithSqliteRepositories()
                .WithAndroidPlatformServices();

            LoggingService = Container.Get<ICoreLoggingService>();
            ApplicationService = Container.Get<ICoreApplicationService>();
            RepositoryService = Container.Get<IRepositoryService>();
            RadioService = Container.Get<IRadioService>();
            PodService = Container.Get<IPodService>();
            IntegrationService = Container.Get<ICoreIntegrationService>();
            
            UnexpectedStopRequestSubject = new Subject<ICoreServices>();
        }

        public async Task StartServices(CancellationToken cancellationToken)
        {
            await LoggingService.StartService(cancellationToken);
            await ApplicationService.StartService(cancellationToken);
            await RepositoryService.StartService(cancellationToken);
            await RadioService.StartService(cancellationToken);
            await PodService.StartService(cancellationToken);
            await IntegrationService.StartService(cancellationToken);
            
            var previousState = ApplicationService.ReadPreferences(new []
            {
                ("CoreAndroidService_StopRequested_RunningServices", string.Empty),
            })[0];
            
            if (!string.IsNullOrEmpty(previousState.Value))
            {
                //TODO: check states of requests - create notifications
                
            }
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}," +
                                      $"{nameof(RepositoryService)},{nameof(RadioService)}," +
                                      $"{nameof(PodService)},{nameof(IntegrationService)}");
        }

        private void StoreRunningServicesValue(string value)
        {
            ApplicationService.StorePreferences(new []
            {
                ("CoreAndroidService_StopRequested_RunningServices", string.Empty),
            });
        }

        public async Task StopServices(CancellationToken cancellationToken)
        {
            await IntegrationService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}," +
                                      $"{nameof(RepositoryService)},{nameof(RadioService)}," +
                                      $"{nameof(PodService)}");
            await PodService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}," +
                                      $"{nameof(RepositoryService)},{nameof(RadioService)}");
            await RadioService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}," +
                                      $"{nameof(RepositoryService)}");
            await RepositoryService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)},{nameof(ApplicationService)}");
            await ApplicationService.StopService(cancellationToken);
            StoreRunningServicesValue($"{nameof(LoggingService)}");
            await LoggingService.StopService(cancellationToken);
            StoreRunningServicesValue(string.Empty);
        }
        
        public void UnexpectedStopRequested()
        {
            UnexpectedStopRequestSubject.OnNext(this);
        }
    }
    
}
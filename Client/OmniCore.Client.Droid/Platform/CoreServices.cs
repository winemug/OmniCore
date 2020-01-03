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
        public ICoreContainer<IServerResolvable> ServerContainer { get; private set; }
        public ICoreLoggingService LoggingService => ServerContainer.Get<ICoreLoggingService>();
        public ICoreApplicationService ApplicationService => ServerContainer.Get<ICoreApplicationService>();
        public IRepositoryService RepositoryService => ServerContainer.Get<IRepositoryService>();
        public IRadioService RadioService => ServerContainer.Get<IRadioService>();
        public IPodService PodService => ServerContainer.Get<IPodService>();
        public ICoreIntegrationService IntegrationService => ServerContainer.Get<ICoreIntegrationService>();

        private readonly ISubject<ICoreServices> UnexpectedStopRequestSubject;

        public IObservable<ICoreServices> OnUnexpectedStopRequest
            => UnexpectedStopRequestSubject.AsObservable();
        
        public CoreServices()
        {
            UnexpectedStopRequestSubject = new Subject<ICoreServices>();
            ServerContainer = Initializer.AndroidServiceContainer(this);
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
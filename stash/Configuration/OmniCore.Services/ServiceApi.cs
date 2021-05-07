using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Services
{
    public class ServiceApi : IServiceApi
    {
        public IObservable<CoreApiStatus> ApiStatus => ApiStatusSubject.AsObservable();
        public IConfigurationService ConfigurationService { get; }
        public IRepositoryService RepositoryService { get; }
        public IPodService PodService { get; }
        public IIntegrationService IntegrationService { get; }
        public IAutomationService AutomationService { get; }

        private readonly ISubject<CoreApiStatus> ApiStatusSubject;
        private readonly ILogger Logger;

        public ServiceApi(
            IRepositoryService repositoryService,
            IPodService podService,
            IAutomationService automationService,
            IIntegrationService integrationService,
            IConfigurationService configurationService,
            ILogger logger)
        {
            RepositoryService = repositoryService;
            PodService = podService;
            AutomationService = automationService;
            IntegrationService = integrationService;
            ConfigurationService = configurationService;
            Logger = logger;
            ApiStatusSubject = new BehaviorSubject<CoreApiStatus>(CoreApiStatus.NotStarted);
        }
        
        public async Task StartServices(CancellationToken cancellationToken)
        {
            Logger.Information("Starting OmniCore services");
            ApiStatusSubject.OnNext(CoreApiStatus.Starting);
            await RepositoryService.StartService(cancellationToken);
            await PodService.StartService(cancellationToken);
            //await AutomationService.StartService(cancellationToken);
            //await IntegrationService.StartService(cancellationToken);
            ApiStatusSubject.OnNext(CoreApiStatus.Started);
            Logger.Information("All services started");
        }

        public async Task StopServices(CancellationToken cancellationToken)
        {
            Logger.Information("Stopping OmniCore services");
            ApiStatusSubject.OnNext(CoreApiStatus.Stopping);
            //await IntegrationService.StopService(cancellationToken);
            //await AutomationService.StopService(cancellationToken);
            await PodService.StopService(cancellationToken);
            await RepositoryService.StopService(cancellationToken);
            ApiStatusSubject.OnNext(CoreApiStatus.Stopped);
            Logger.Information("Services stopped");
        }
    }
}
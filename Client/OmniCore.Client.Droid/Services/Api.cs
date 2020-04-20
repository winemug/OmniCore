using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client.Droid.Services
{
    public class Api : IApi
    {
        public IObservable<CoreApiStatus> ApiStatus => ApiStatusSubject.AsObservable();
        public IConfigurationService ConfigurationService { get; }
        public IRepositoryService RepositoryService { get; }
        public IPodService PodService { get; }
        public IIntegrationService IntegrationService { get; }
        public IAutomationService AutomationService { get; }

        private readonly ISubject<CoreApiStatus> ApiStatusSubject;
        private readonly IServiceFunctions ServiceFunctions;

        public Api(
            IServiceFunctions serviceFunctions,
            IRepositoryService repositoryService,
            IPodService podService,
            IAutomationService automationService,
            IIntegrationService integrationService,
            IConfigurationService configurationService)
        {
            ServiceFunctions = serviceFunctions;
            RepositoryService = repositoryService;
            PodService = podService;
            AutomationService = automationService;
            IntegrationService = integrationService;
            ConfigurationService = configurationService;
            ApiStatusSubject = new BehaviorSubject<CoreApiStatus>(CoreApiStatus.Starting);
        }
        
        public async Task StartServices(CancellationToken cancellationToken)
        {
            ApiStatusSubject.OnNext(CoreApiStatus.Starting);
            await RepositoryService.StartService(cancellationToken);
            await PodService.StartService(cancellationToken);
            //await AutomationService.StartService(cancellationToken);
            //await IntegrationService.StartService(cancellationToken);
            ApiStatusSubject.OnNext(CoreApiStatus.Started);
        }

        public async Task StopServices(CancellationToken cancellationToken)
        {
            //await IntegrationService.StopService(cancellationToken);
            //await AutomationService.StopService(cancellationToken);
            await PodService.StopService(cancellationToken);
            await RepositoryService.StopService(cancellationToken);
        }
    }
}
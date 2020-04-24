using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Services.Configuration;

namespace OmniCore.Client.Droid.Services
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
        private readonly IUserActivity UserActivity;

        public ServiceApi(
            IRepositoryService repositoryService,
            IPodService podService,
            IAutomationService automationService,
            IIntegrationService integrationService,
            IConfigurationService configurationService,
            IUserActivity userActivity)
        {
            RepositoryService = repositoryService;
            PodService = podService;
            AutomationService = automationService;
            IntegrationService = integrationService;
            ConfigurationService = configurationService;
            UserActivity = userActivity;
            ApiStatusSubject = new BehaviorSubject<CoreApiStatus>(CoreApiStatus.NotStarted);
        }
        
        public async Task StartServices(CancellationToken cancellationToken)
        {
            ApiStatusSubject.OnNext(CoreApiStatus.Starting);
            await UserActivity.StartForegroundTaskService(cancellationToken);
            await RepositoryService.StartService(cancellationToken);
            await PodService.StartService(cancellationToken);
            //await AutomationService.StartService(cancellationToken);
            //await IntegrationService.StartService(cancellationToken);
            ApiStatusSubject.OnNext(CoreApiStatus.Started);
        }

        public async Task StopServices(CancellationToken cancellationToken)
        {
            ApiStatusSubject.OnNext(CoreApiStatus.Stopping);
            //await IntegrationService.StopService(cancellationToken);
            //await AutomationService.StopService(cancellationToken);
            await PodService.StopService(cancellationToken);
            await RepositoryService.StopService(cancellationToken);
            await UserActivity.StopForegroundTaskService(cancellationToken);
            ApiStatusSubject.OnNext(CoreApiStatus.Stopped);
        }
    }
}
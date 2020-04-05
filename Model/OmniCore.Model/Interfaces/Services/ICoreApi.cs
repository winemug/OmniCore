using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreApi : IServerResolvable, IClientResolvable
    {
        IObservable<CoreApiStatus> ApiStatus { get; }
        ICoreLoggingFunctions LoggingFunctions { get; }
        ICoreApplicationFunctions ApplicationFunctions { get; }
        ICoreNotificationFunctions NotificationFunctions { get; }
        ICoreRepositoryService RepositoryService { get; }
        ICoreConfigurationService ConfigurationService { get; }
        ICorePodService PodService { get; }
        ICoreIntegrationService IntegrationService { get; }
        ICoreAutomationService AutomationService { get; }
        Task StartServices(CancellationToken cancellationToken);
        Task StopServices(CancellationToken cancellationToken);
    }
}
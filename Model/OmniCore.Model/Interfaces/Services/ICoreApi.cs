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
        Task StartServices(CancellationToken cancellationToken);
        Task StopServices(CancellationToken cancellationToken);
        ICoreLoggingFunctions LoggingFunctions { get; }
        ICoreApplicationFunctions ApplicationFunctions { get; }
        ICoreNotificationFunctions NotificationFunctions { get; }
        ICoreRepositoryService CoreRepositoryService { get; }
        ICoreConfigurationService CoreConfigurationService { get; }
        ICorePodService CorePodService { get; }
        ICoreIntegrationService IntegrationService { get; }
    }
}

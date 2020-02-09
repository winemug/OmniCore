using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Base;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreApi : IServerResolvable, IClientResolvable
    {
        Task StartServices(CancellationToken cancellationToken);
        Task StopServices(CancellationToken cancellationToken);
        ICoreLoggingFunctions LoggingFunctions { get; }
        ICoreApplicationFunctions ApplicationFunctions { get; }
        ICoreNotificationFunctions NotificationFunctions { get; }
        IRepositoryService RepositoryService { get; }
        IConfigurationService ConfigurationService { get; }
        IPodService PodService { get; }
        ICoreIntegrationService IntegrationService { get; }
    }
}

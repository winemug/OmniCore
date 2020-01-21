using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ICoreApi : IServerResolvable, IClientResolvable
    {
        Task StartServices(CancellationToken cancellationToken);
        Task StopServices(CancellationToken cancellationToken);
        ICoreLoggingFunctions LoggingFunctions { get; }
        ICoreApplicationFunctions ApplicationFunctions { get; }
        ICoreNotificationFunctions NotificationFunctions { get; }
        IRepositoryService RepositoryService { get; }
        IRadioService RadioService { get; }
        IPodService PodService { get; }
        ICoreIntegrationService IntegrationService { get; }
    }
}

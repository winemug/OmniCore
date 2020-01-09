using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ICoreServiceApi : IServerResolvable, IClientResolvable
    {
        Task StartServices(CancellationToken cancellationToken);
        Task StopServices(CancellationToken cancellationToken);
        ICoreLoggingFunctions LoggingFunctions { get; }
        ICoreApplicationFunctions ApplicationFunctions { get; }
        IRepositoryService RepositoryService { get; }
        IRadioService RadioService { get; }
        IPodService PodService { get; }
        ICoreIntegrationService IntegrationService { get; }
    }
}

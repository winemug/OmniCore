using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Interfaces
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

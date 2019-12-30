using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreBootstrapper
    {
        void StartServices(CancellationToken cancellationToken);
        void StopServices(CancellationToken cancellationToken);

        ICoreContainer Container { get; }
        ICoreLoggingService LoggingService { get; }
        ICoreApplicationService ApplicationService { get; }
        IRepositoryService RepositoryService { get; }
        IRadioService RadioService { get; }
        IPodService PodService { get; }
        ICoreIntegrationService IntegrationService { get; }
    }
}

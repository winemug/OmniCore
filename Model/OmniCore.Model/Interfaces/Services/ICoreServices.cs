using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreServices
    {
        Task StartServices(CancellationToken cancellationToken);
        Task StopServices(CancellationToken cancellationToken);
        IObservable<ICoreServices> OnUnexpectedStopRequest { get; }
        ICoreContainer Container { get; }
        ICoreLoggingService LoggingService { get; }
        ICoreApplicationService ApplicationService { get; }
        IRepositoryService RepositoryService { get; }
        IRadioService RadioService { get; }
        IPodService PodService { get; }
        ICoreIntegrationService IntegrationService { get; }
    }
}

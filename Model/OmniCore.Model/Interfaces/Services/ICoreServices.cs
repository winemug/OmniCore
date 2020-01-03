using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreServices : IClientResolvable, IServerResolvable //TODO: preliminary before change
    {
        Task StartServices(CancellationToken cancellationToken);
        Task StopServices(CancellationToken cancellationToken);
        ICoreLoggingService LoggingService { get; }
        ICoreApplicationService ApplicationService { get; }
        IRepositoryService RepositoryService { get; }
        IRadioService RadioService { get; }
        IPodService PodService { get; }
        ICoreIntegrationService IntegrationService { get; }
        
        //TODO: temporary as above
        ICoreContainer<IServerResolvable> ServerContainer { get; }
        IObservable<ICoreServices> OnUnexpectedStopRequest { get; }
        void UnexpectedStopRequested();
    }
}

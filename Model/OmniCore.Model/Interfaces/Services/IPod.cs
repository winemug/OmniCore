using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IPod: IDisposable
    {
        PodEntity Entity { get; }
        PodRunningState RunningState { get; }
        Task Archive(CancellationToken cancellationToken);
        IObservable<IPod> WhenPodArchived();
        Task<IList<IPodRequest>> GetActiveRequests();
        Task<IPodActivationRequest> ActivationRequest();
        Task<IPodBolusRequest> BolusRequest();
        Task<IPodDeliveryCancellationRequest> CancellationRequest();
        Task<IPodScheduledDeliveryRequest> ScheduledDeliveryRequest();
    }
}
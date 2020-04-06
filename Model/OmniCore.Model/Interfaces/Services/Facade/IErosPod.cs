using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IErosPod : IPod
    {
        IObservable<IEnumerable<IErosRadio>> WhenRadiosUpdated();
        Task UpdateRadioList(IEnumerable<IErosRadio> radios, CancellationToken cancellationToken);
        Task<IPodRequest> Activate(IErosRadio radio, CancellationToken cancellationToken);
        Task<IPodRequest> Acquire(IErosRadio radio, CancellationToken cancellationToken);
        Task<IPodRequest> VerifyIdentity(uint lotNumber, uint serialNumber, CancellationToken cancellationToken);
    }
}
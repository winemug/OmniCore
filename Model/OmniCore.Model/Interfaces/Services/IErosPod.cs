using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IErosPod : IPod
    {
        Task UpdateRadioList(IEnumerable<IErosRadio> radios, CancellationToken cancellationToken);
        Task<IPodTask> Activate(IErosRadio radio, CancellationToken cancellationToken);
        Task<IPodTask> Acquire(IErosRadio radio, CancellationToken cancellationToken);
        Task<IPodTask> VerifyIdentity(uint lotNumber, uint serialNumber, CancellationToken cancellationToken);
    }
}
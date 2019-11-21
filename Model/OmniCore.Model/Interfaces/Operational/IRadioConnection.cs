using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Operational
{
    public interface IRadioConnection : IDisposable
    {
        IRadioPeripheralLease Lease { get; }
        Task<bool> Initialize(CancellationToken cancellationToken);
        Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken);
    }
}

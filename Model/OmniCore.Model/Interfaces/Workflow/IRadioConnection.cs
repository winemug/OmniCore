using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadioConnection : IDisposable
    {
        IRadioPeripheralLease Lease { get; set; }
        IRadio Radio { get; set; }
        Task<bool> Initialize(CancellationToken cancellationToken);
        Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken);
    }
}
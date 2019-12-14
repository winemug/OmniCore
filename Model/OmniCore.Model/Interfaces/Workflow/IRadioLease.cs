using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadioLease : IDisposable
    {
        IRadioPeripheralLease PeripheralLease { get; set; }
        IRadio Radio { get; set; }
        Task Configure(IRadioConfiguration radioConfiguration, CancellationToken cancellationToken);
        Task Identify(CancellationToken cancellationToken);
        Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken);
    }
}
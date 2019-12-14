using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheral : IDisposable
    {
        Guid PeripheralUuid { get; }
        string PeripheralName { get; }
        Task<IRadioPeripheralLease> Lease(CancellationToken cancellationToken);
        Task<int> ReadRssi(CancellationToken cancellationToken);
    }
}

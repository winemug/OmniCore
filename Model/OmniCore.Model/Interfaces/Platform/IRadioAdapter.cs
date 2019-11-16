using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioAdapter
    {
        Task<List<IRadioPeripheralScanResult>> GetKnownPeripherals(Guid serviceId, CancellationToken cancellationToken);
        IObservable<IRadioPeripheralScanResult> ScanPeripherals(Guid serviceId, CancellationToken cancellationToken);
        Task<IRadioPeripheralLease> LeasePeripheral(Guid id, CancellationToken cancellationToken);
    }
}

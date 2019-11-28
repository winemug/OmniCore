using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioAdapter
    {
        Task<List<IRadioPeripheralResult>> GetKnownPeripherals(Guid serviceId, CancellationToken cancellationToken);
        IObservable<IRadioPeripheralResult> ScanPeripherals(Guid serviceId, CancellationToken cancellationToken);
        Task<IRadioPeripheralLease> LeasePeripheral(Guid peripheralUuid, CancellationToken cancellationToken);
    }
}

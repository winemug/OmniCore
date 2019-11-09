using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioAdapter
    {
        Task<bool> TryEnable();
        Task<bool> TryDisable();
        IObservable<IRadioPeripheralScanResult> ScanPeripherals(Guid serviceId);
        Task<List<IRadioPeripheral>> GetConnectedPeripherals(Guid serviceId);
        Task<IRadioPeripheral> GetPeripheral(Guid id, CancellationToken cancellationToken);
    }
}

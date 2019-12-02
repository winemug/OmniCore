using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Simulation.Radios
{
    public class RadioAdapter : IRadioAdapter
    {
        public Task<List<IRadioPeripheralResult>> GetKnownPeripherals(Guid serviceId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IObservable<IRadioPeripheralResult> ScanPeripherals(Guid serviceId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IRadioPeripheralLease> LeasePeripheral(Guid peripheralUuid, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;

namespace OmniCore.Simulation.Radios
{
    public class RadioAdapter : IRadioAdapter
    {
        public Task TryEnsureAdapterEnabled(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<bool> TryEnableAdapter(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<bool> TryDisableAdapter(CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public IObservable<IRadioPeripheralResult> FindPeripherals(Guid serviceId)
        {
            throw new NotImplementedException();
        }

        public Task<IRadioPeripheralResult> FindPeripheral(Guid peripheralUuid, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

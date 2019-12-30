using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Simulation.Radios
{
    public class RadioAdapter : IRadioAdapter
    {
        public Task TryEnsureAdapterEnabled(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> TryEnableAdapter(CancellationToken cancellationToken)
        {
            return true;
        }

        public async Task<bool> TryDisableAdapter(CancellationToken cancellationToken)
        {
            return false;
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

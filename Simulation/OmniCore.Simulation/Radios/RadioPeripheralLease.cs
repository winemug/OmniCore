using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Simulation.Radios
{
    public class RadioPeripheralLease : IRadioPeripheralLease
    {
        public IObservable<IRadioPeripheralLease> WhenConnected()
        {
            throw new NotImplementedException();
        }

        public IObservable<Exception> WhenConnectionFailed()
        {
            throw new NotImplementedException();
        }

        public IObservable<IRadioPeripheralLease> WhenDisconnected()
        {
            throw new NotImplementedException();
        }

        public Task<IRadioPeripheralCharacteristic[]> GetCharacteristics(Guid serviceId, Guid[] characteristicIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Connect(bool autoConnect, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Disconnect(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task<int> ReadRssi()
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

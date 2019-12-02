using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Simulation.Radios
{
    public class RadioPeripheralCharacteristic : IRadioPeripheralCharacteristic
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> Read(TimeSpan timeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Write(byte[] data, TimeSpan timeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IObservable<IRadioPeripheralCharacteristic> WhenNotificationReceived()
        {
            throw new NotImplementedException();
        }
    }
}

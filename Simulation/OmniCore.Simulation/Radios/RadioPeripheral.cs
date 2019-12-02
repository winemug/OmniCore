using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Simulation.Radios
{
    public class RadioPeripheral : IRadioPeripheral
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Guid PeripheralId { get; }
        public string PeripheralName { get; }
        public bool IsConnected { get; }
        public IObservable<IRadioPeripheral> WhenConnected()
        {
            throw new NotImplementedException();
        }

        public IObservable<Exception> WhenConnectionFailed()
        {
            throw new NotImplementedException();
        }

        public IObservable<IRadioPeripheral> WhenDisconnected()
        {
            throw new NotImplementedException();
        }

        public IObservable<IRadioPeripheral> WhenDeviceChanged()
        {
            throw new NotImplementedException();
        }

        public IObservable<IRadioPeripheral> WhenDeviceLost()
        {
            throw new NotImplementedException();
        }

        public Task<IRadioPeripheralCharacteristic[]> GetCharacteristics(Guid serviceId, Guid[] characteristicIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Connect(bool autoConnect, CancellationToken cancellationToken)
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
    }
}

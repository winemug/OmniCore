using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheral : IDisposable
    {
        Guid PeripheralId { get; }
        string PeripheralName { get; }
        bool IsConnected { get; }
        IObservable<IRadioPeripheral> WhenConnected();
        IObservable<Exception> WhenConnectionFailed();
        IObservable<IRadioPeripheral> WhenDisconnected();
        IObservable<IRadioPeripheral> WhenDeviceChanged();
        IObservable<IRadioPeripheral> WhenDeviceLost();
        Task<IRadioPeripheralCharacteristic[]> GetCharacteristics(Guid serviceId, Guid[] characteristicIds, CancellationToken cancellationToken);
        Task<bool> Connect(bool autoConnect, CancellationToken cancellationToken);
        Task Disconnect(TimeSpan timeout);
        Task<int> ReadRssi();
    }
}

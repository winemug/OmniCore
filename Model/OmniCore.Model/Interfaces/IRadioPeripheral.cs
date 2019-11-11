using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioPeripheral : IDisposable
    {
        Guid PeripheralId { get; }
        string PeripheralName { get; }
        IObservable<IRadioPeripheral> WhenConnected();
        IObservable<Exception> WhenConnectionFailed();
        IObservable<IRadioPeripheral> WhenDisconnected();
        IObservable<IRadioPeripheral> WhenDeviceChanged();
        IObservable<IRadioPeripheral> WhenDeviceLost();
        Task<bool> Connect(CancellationToken cancellationToken);
        Task Disconnect(TimeSpan timeout);
        Task<int> ReadRssi();
    }
}

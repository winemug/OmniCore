using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioPeripheral : IDisposable
    {
        Guid PeripheralId { get; }
        string PeripheralName { get; }
        Task<bool> IsConnected();
        IObservable<IRadioPeripheral> WhenConnected();
        IObservable<Exception> WhenConnectionFailed();
        IObservable<IRadioPeripheral> WhenDisconnected();
        Task<bool> Connect();
        Task Disconnect();
        Task<int> ReadRssi();
    }
}

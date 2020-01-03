using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralLease : IDisposable, IServerResolvable
    {
        IObservable<IRadioPeripheralLease> WhenConnected();
        IObservable<Exception> WhenConnectionFailed();
        IObservable<IRadioPeripheralLease> WhenDisconnected();
        Task<IRadioPeripheralCharacteristic[]> GetCharacteristics(Guid serviceId, Guid[] characteristicIds, CancellationToken cancellationToken);
        Task Connect(bool autoConnect, CancellationToken cancellationToken);
        Task Disconnect(CancellationToken cancellationToken);
    }
}

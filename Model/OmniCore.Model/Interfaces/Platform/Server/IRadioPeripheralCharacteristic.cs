using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioPeripheralCharacteristic : IDisposable, IServerResolvable
    {
        Guid Uuid { get; }
        Task<byte[]> Read(CancellationToken cancellationToken);
        Task Write(byte[] data, CancellationToken cancellationToken);
        IObservable<IRadioPeripheralCharacteristic> WhenNotificationReceived();
        Task WriteWithoutResponse(byte[] data, CancellationToken cancellationToken);
    }
}

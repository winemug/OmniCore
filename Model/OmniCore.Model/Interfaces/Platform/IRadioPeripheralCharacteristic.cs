using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRadioPeripheralCharacteristic : IDisposable
    {
        Guid Uuid { get; }
        Task<byte[]> Read(CancellationToken cancellationToken);
        Task Write(byte[] data, CancellationToken cancellationToken);
        IObservable<IRadioPeripheralCharacteristic> WhenNotificationReceived();
        Task WriteWithoutResponse(byte[] data, CancellationToken cancellationToken);
    }
}

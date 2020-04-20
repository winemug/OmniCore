using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IBlePeripheralConnection: IDisposable
    {
        Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid,
            CancellationToken cancellationToken);

        Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken);

        IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid ServiceUuid, Guid CharacteristicUuid);

        Task WriteToCharacteristicWithoutResponse(Guid ServiceUuid, Guid CharacteristicUuid, byte[] data,
            CancellationToken cancellationToken);
    }
}
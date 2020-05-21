using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client.Console.Platform
{
    public class WinBlePeripheralConnection : IBlePeripheralConnection
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IBlePeripheral Peripheral { get; }
        public async Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid ServiceUuid, Guid CharacteristicUuid)
        {
            throw new NotImplementedException();
        }

        public async Task WriteToCharacteristicWithoutResponse(Guid ServiceUuid, Guid CharacteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
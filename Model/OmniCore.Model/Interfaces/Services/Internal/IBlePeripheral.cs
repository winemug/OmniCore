using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IBlePeripheral : IServerResolvable
    {
        Guid PeripheralUuid { get; }
        Guid PrimaryServiceUuid { get; }
        IObservable<string> Name { get; }
        IObservable<PeripheralState> State { get; }
        IObservable<PeripheralConnectionState> ConnectionState { get; }
        IObservable<int> Rssi{ get; }
        TimeSpan? RssiAutoUpdateInterval { get; set; }
        IObservable<IBlePeripheral> Locate();
        Task Connect(bool autoConnect, CancellationToken cancellationToken);
        Task Disconnect(CancellationToken cancellationToken);
        Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid, CancellationToken cancellationToken);
        Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data, CancellationToken cancellationToken);
        IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid ServiceUuid, Guid CharacteristicUuid);
        Task WriteToCharacteristicWithoutResponse(Guid ServiceUuid, Guid CharacteristicUuid, byte[] data, CancellationToken cancellationToken);
    }
}

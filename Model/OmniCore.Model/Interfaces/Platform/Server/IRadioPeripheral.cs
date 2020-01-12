using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IRadioPeripheral : ILeaseable<IRadioPeripheral>, IServerResolvable
    {
        Guid PeripheralUuid { get; }
        Guid[] ServiceUuids { get; }
        IObservable<string> Name { get; }
        IObservable<PeripheralState> State { get; }
        IObservable<PeripheralConnectionState> ConnectionState { get; }
        IObservable<int> Rssi{ get; }

        TimeSpan? RssiAutoUpdateInterval { get; set; }
        void RequestRssi();
        Task Locate(CancellationToken cancellationToken);
        Task Connect(bool autoConnect, CancellationToken cancellationToken);
        Task Disconnect(CancellationToken cancellationToken);
        Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid, CancellationToken cancellationToken);
        Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data, CancellationToken cancellationToken);
        IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid ServiceUuid, Guid CharacteristicUuid);
        Task WriteToCharacteristicWithoutResponse(Guid ServiceUuid, Guid CharacteristicUuid, byte[] data, CancellationToken cancellationToken);
    }
}

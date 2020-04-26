using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IBlePeripheral 
    {
        Guid PeripheralUuid { get; }
        Guid PrimaryServiceUuid { get; }
        string Name { get; set; }
        (int Rssi, DateTimeOffset Date)? Rssi { get; }

        (PeripheralDiscoveryState State, DateTimeOffset Date)
            DiscoveryState { get; }

        (PeripheralConnectionState State, DateTimeOffset Date)
            ConnectionState { get; }

        IObservable<string> WhenNameUpdated();
        IObservable<int> WhenRssiReceived();
        Task<int> ReadRssi(CancellationToken cancellationToken);
        Task<int> RequestMtu(int size, CancellationToken cancellationToken);
        Task Discover(CancellationToken cancellationToken);
        IObservable<PeripheralDiscoveryState> WhenDiscoveryStateChanged();
        IObservable<PeripheralConnectionState> WhenConnectionStateChanged();

        Task<IBlePeripheralConnection> GetConnection(
            BlePeripheralOptions peripheralOptions,
            CancellationToken cancellationToken);
    }
}
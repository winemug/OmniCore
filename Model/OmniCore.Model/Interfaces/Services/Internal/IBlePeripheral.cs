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
        string Name { get; set; }
        (int Rssi, DateTimeOffset Date)? Rssi { get; }
        (PeripheralDiscoveryState State, DateTimeOffset Date)
            DiscoveryState { get; }

        (PeripheralConnectionState State, DateTimeOffset Date)
            ConnectionState { get; }
        
        IObservable<string> WhenNameUpdated();
        IObservable<int> WhenRssiReceived();
        Task<int> ReadRssi(CancellationToken cancellationToken);
        Task Discover(CancellationToken cancellationToken);
        IObservable<PeripheralDiscoveryState> WhenDiscoveryStateChanged();
        IObservable<PeripheralConnectionState> WhenConnectionStateChanged();
        Task<IBlePeripheralConnection> GetConnection(
            bool autoConnect,
            bool stayConnected,
            TimeSpan discoveryTimeout,
            TimeSpan connectTimeout,
            TimeSpan characteristicDiscoveryTimeout,
            CancellationToken cancellationToken);
    }
}

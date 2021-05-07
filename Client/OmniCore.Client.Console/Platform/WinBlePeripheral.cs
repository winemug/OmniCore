using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client.Console.Platform
{
    public class WinBlePeripheral : IBlePeripheral
    {
        public Guid PeripheralUuid { get; set; }
        public Guid PrimaryServiceUuid { get; set; }
        public string Name { get; set; }
        public (int Rssi, DateTimeOffset Date)? Rssi { get; }
        public (PeripheralDiscoveryState State, DateTimeOffset Date) DiscoveryState { get; }
        public (PeripheralConnectionState State, DateTimeOffset Date) ConnectionState { get; }
        public IObservable<string> WhenNameUpdated()
        {
            return Observable.Never<string>();
        }

        public IObservable<int> WhenRssiReceived()
        {
            return Observable.Never<int>();
        }

        public async Task<int> ReadRssi(CancellationToken cancellationToken)
        {
            return 0;
        }

        public async Task<int> RequestMtu(int size, CancellationToken cancellationToken)
        {
            return 0;
        }

        public async Task Discover(CancellationToken cancellationToken)
        {
        }

        public IObservable<PeripheralDiscoveryState> WhenDiscoveryStateChanged()
        {
            return Observable.Never<PeripheralDiscoveryState>();
        }

        public IObservable<PeripheralConnectionState> WhenConnectionStateChanged()
        {
            return Observable.Never<PeripheralConnectionState>();
        }

        public async Task<IBlePeripheralConnection> GetConnection(BleOptions options, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
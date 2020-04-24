using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IErosRadio: IDisposable
    {
        IErosRadioProvider Provider { get; }
        RadioEntity Entity { get; }
        RadioType Type { get; }
        string Address { get; }
        string Description { get; }
        IObservable<string> Name { get; }
        IObservable<PeripheralDiscoveryState> DiscoveryState { get; }
        IObservable<PeripheralConnectionState> ConnectionState { get; }
        IObservable<int> Rssi { get; }
        Task Identify(CancellationToken cancellationToken);
        RadioOptions Options { get; }
        Task<byte[]> GetResponse(IPodRequest request, CancellationToken cancellationToken, RadioOptions options);
        // Task<(byte Rssi, byte[] Data)> DebugGetPacket(uint timeoutMilliseconds, CancellationToken cancellationToken);
        void StartMonitoring();
    }
}
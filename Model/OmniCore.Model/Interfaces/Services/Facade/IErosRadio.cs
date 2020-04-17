using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IErosRadio : IServiceInstance, IDisposable
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
        Task<byte[]> GetResponse(IErosPodRequest request, CancellationToken cancellationToken, RadioOptions options);
        // Task<(byte Rssi, byte[] Data)> DebugGetPacket(uint timeoutMilliseconds, CancellationToken cancellationToken);

    }
}
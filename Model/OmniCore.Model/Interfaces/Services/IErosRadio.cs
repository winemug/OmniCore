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
        RadioEntity Entity { get; }
        RadioType Type { get; }
        string Address { get; }
        IObservable<string> Name { get; }
        IObservable<PeripheralDiscoveryState> DiscoveryState { get; }
        IObservable<PeripheralConnectionState> ConnectionState { get; }
        IObservable<int> Rssi { get; }
        RadioOptions DefaultOptions { get; }
        Task SetDefaultOptions(RadioOptions options, CancellationToken cancellationToken);
        Task SetName(string newName, CancellationToken cancellationToken);
        Task PerformHealthCheck(CancellationToken cancellationToken);
        Task<IRadioConnection> GetConnection(CancellationToken cancellationToken);
        Task<IRadioConnection> GetConnection(RadioOptions options, CancellationToken cancellationToken);
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IRadio
    {
        RadioType Type { get; }
        string Address { get; }
        string Description { get; }
        IObservable<string> Name { get; }
        IObservable<PeripheralDiscoveryState> DiscoveryState { get; }
        IObservable<PeripheralConnectionState> ConnectionState { get; }
        IObservable<int> Rssi { get; }
        Task Identify(CancellationToken cancellationToken);
    }
}
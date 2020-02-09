using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IBlePeripheralAdapter : IServerResolvable
    {
        IObservable<bool> IsScanning { get; }
        Task TryEnsureAdapterEnabled(CancellationToken cancellationToken);
        Task<bool> TryEnableAdapter(CancellationToken cancellationToken);
        Task<bool> TryDisableAdapter(CancellationToken cancellationToken);
        IObservable<IBlePeripheral> FindErosRadioPeripherals();
        IBlePeripheral GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid);
        IObservable<IBlePeripheralAdapter> WhenDiscoveryStarting();
        IObservable<IBlePeripheralAdapter> WhenDiscoveryFinished();
        IObservable<IBlePeripheralAdapter> WhenAdapterDisabled();
        IObservable<IBlePeripheralAdapter> WhenAdapterEnabled();

    }
}

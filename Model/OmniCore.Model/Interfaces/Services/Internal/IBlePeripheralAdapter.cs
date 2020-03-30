using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IBlePeripheralAdapter : IServerResolvable
    {
        Task TryEnsureAdapterEnabled(CancellationToken cancellationToken);
        Task<bool> TryEnableAdapter(CancellationToken cancellationToken);
        Task<IDisposable> PeripheralConnectionLock(CancellationToken cancellationToken);
        IObservable<IBlePeripheral> FindErosRadioPeripherals();
        IBlePeripheral GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid);
        IObservable<IBlePeripheralAdapter> WhenAdapterDisabled();
        IObservable<IBlePeripheralAdapter> WhenAdapterEnabled();
        IObservable<IBlePeripheralAdapter> WhenScanStarted { get; }
        IObservable<IBlePeripheralAdapter> WhenScanFinished { get; }

    }
}

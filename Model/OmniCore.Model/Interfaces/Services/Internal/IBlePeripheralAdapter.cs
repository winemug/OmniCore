using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IBlePeripheralAdapter 
    {
        Task<IBlePeripheral> GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid);
        IObservable<IBlePeripheralAdapter> WhenScanStarted { get; }
        IObservable<IBlePeripheralAdapter> WhenScanFinished { get; }
        Task TryEnsureAdapterEnabled(CancellationToken cancellationToken);
        Task<bool> TryEnableAdapter(CancellationToken cancellationToken);
        Task<IDisposable> PeripheralConnectionLock(CancellationToken cancellationToken);
        IObservable<IBlePeripheral> FindErosRadioPeripherals();
        IObservable<IBlePeripheralAdapter> WhenAdapterDisabled();
        IObservable<IBlePeripheralAdapter> WhenAdapterEnabled();
        void InvalidatePeripheralState(IBlePeripheral peripheral);
    }
}
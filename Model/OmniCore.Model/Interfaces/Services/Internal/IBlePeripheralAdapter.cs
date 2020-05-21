using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IBlePeripheralAdapter 
    {
        Task<IBlePeripheral> GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid);
        Task TryEnsureAdapterEnabled(CancellationToken cancellationToken);
        Task<bool> TryEnableAdapter(CancellationToken cancellationToken);
        IObservable<IBlePeripheral> FindErosRadioPeripherals();
        IObservable<IBlePeripheralAdapter> WhenAdapterDisabled();
        IObservable<IBlePeripheralAdapter> WhenAdapterEnabled();
        void InvalidatePeripheralState(IBlePeripheral peripheral);
    }
}
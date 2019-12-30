using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRadioAdapter
    {
        Task TryEnsureAdapterEnabled(CancellationToken cancellationToken);
        Task<bool> TryEnableAdapter(CancellationToken cancellationToken);
        Task<bool> TryDisableAdapter(CancellationToken cancellationToken);
        IObservable<IRadioPeripheralResult> FindPeripherals(Guid serviceId);
        Task<IRadioPeripheralResult> FindPeripheral(Guid peripheralUuid, CancellationToken cancellationToken);
    }
}

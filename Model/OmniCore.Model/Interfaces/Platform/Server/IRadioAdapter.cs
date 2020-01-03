using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioAdapter : IServerResolvable
    {
        Task TryEnsureAdapterEnabled(CancellationToken cancellationToken);
        Task<bool> TryEnableAdapter(CancellationToken cancellationToken);
        Task<bool> TryDisableAdapter(CancellationToken cancellationToken);
        IObservable<IRadioPeripheralResult> FindPeripherals(Guid serviceId);
        Task<IRadioPeripheralResult> FindPeripheral(Guid peripheralUuid, CancellationToken cancellationToken);
    }
}

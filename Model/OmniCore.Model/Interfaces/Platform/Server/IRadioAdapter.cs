using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IRadioAdapter : IServerResolvable
    {
        Task TryEnsureAdapterEnabled(CancellationToken cancellationToken);
        Task<bool> TryEnableAdapter(CancellationToken cancellationToken);
        Task<bool> TryDisableAdapter(CancellationToken cancellationToken);
        IObservable<IRadioPeripheral> FindPeripherals();
        IRadioPeripheral GetPeripheral(Guid peripheralUuid);
    }
}

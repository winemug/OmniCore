using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client.Console.Platform
{
    public class WinBlePeripheralAdapter : IBlePeripheralAdapter
    {
        private readonly IContainer Container;
        
        public WinBlePeripheralAdapter(
            IContainer container)
        {
            Container = container;
        }
        
        public async Task<IBlePeripheral> GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid)
        {
            throw new NotImplementedException();
        }
        public async Task TryEnsureAdapterEnabled(CancellationToken cancellationToken)
        {
        }

        public async Task<bool> TryEnableAdapter(CancellationToken cancellationToken)
        {
            return true;
        }

        public IObservable<IBlePeripheral> FindErosRadioPeripherals()
        {
            throw new NotImplementedException();
        }

        public IObservable<IBlePeripheralAdapter> WhenAdapterDisabled()
        {
            return Observable.Never<IBlePeripheralAdapter>();
        }

        public IObservable<IBlePeripheralAdapter> WhenAdapterEnabled()
        {
            return Observable.Return(this);
        }

        public void InvalidatePeripheralState(IBlePeripheral peripheral)
        {
        }
    }
}
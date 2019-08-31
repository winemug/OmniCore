using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkRadioProvider : IRadioProvider
    {

        private readonly Guid RileyLinkServiceUUID = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        private readonly IRadioAdapter _radioAdapter;

        public RileyLinkRadioProvider(IRadioAdapter radioAdapter)
        {
            _radioAdapter = radioAdapter;
        }

        public IObservable<IRadio> ListRadios()
        {
            return Observable.Create<IRadio>((IObserver<IRadio> observer) =>
            {
                var disposable = _radioAdapter.ScanPeripherals(RileyLinkServiceUUID)
                    .Subscribe(peripheral =>
                    {
                        observer.OnNext(new RileyLinkRadio(peripheral));
                    });
                return Disposable.Create(() => { disposable.Dispose(); });
            });
        }

        public async Task<IRadioPeripheral> GetByProviderSpecificId(string id)
        {
            if (!id.StartsWith("RLL"))
                return null;

            var peripheralId = Guid.Parse(id.Substring(3));
            return await _radioAdapter.GetPeripheral(peripheralId);
        }
    }
}

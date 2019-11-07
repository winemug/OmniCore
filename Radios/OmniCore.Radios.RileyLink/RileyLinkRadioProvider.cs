using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using OmniCore.Repository;
using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioProvider : IRadioProvider
    {

        private readonly Guid RileyLinkServiceUUID = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        private readonly IRadioAdapter _radioAdapter;

        public RileyLinkRadioProvider(IRadioAdapter radioAdapter)
        {
            _radioAdapter = radioAdapter;
        }

        public IObservable<Radio> ListRadios()
        {
            return Observable.Create<Radio>((IObserver<Radio> observer) =>
            {
                var disposable = _radioAdapter.ScanPeripherals(RileyLinkServiceUUID)
                    .Subscribe(async peripheral =>
                    {
                        var re = await GetRadioEntity(peripheral);
                        using(var rcr = new RadioConnectionRepository())
                        {
                            await rcr.Create(new RadioConnection
                            {
                                RadioId = re.Id.Value,
                                EventType = RadioConnectionEvent.Scan,
                                Successful = true,
                                Rssi = peripheral.Rssi
                            });
                        }
                        observer.OnNext(re);
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

        private async Task<Radio> GetRadioEntity(IRadioPeripheral peripheral)
        {
            var rlr = new RileyLinkRadio(peripheral);

            using(var rr = new RadioRepository())
            {
                var entity = await rr.GetByProviderSpecificId(rlr.ProviderSpecificId);
                if (entity == null)
                {
                    entity = await rr.Create(new Radio
                    {
                        DeviceId = rlr.DeviceId,
                        DeviceName = rlr.DeviceName,
                        DeviceType = rlr.DeviceType,
                        ProviderSpecificId = rlr.ProviderSpecificId
                    });
                }
                return entity;
            }
        }

    }
}

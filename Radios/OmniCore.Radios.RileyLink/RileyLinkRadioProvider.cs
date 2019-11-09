using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
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

        private readonly IRadioAdapter RadioAdapter;

        public RileyLinkRadioProvider(IRadioAdapter radioAdapter)
        {
            RadioAdapter = radioAdapter;
        }

        public IObservable<Radio> ListRadios()
        {
            return Observable.Create<Radio>(async (IObserver<Radio> observer) =>
            {
                var peripheralIds  = new HashSet<Guid>();
                var connectedPeripherals = await RadioAdapter.GetConnectedPeripherals(RileyLinkServiceUUID);
                foreach(var connectedPeripheral in connectedPeripherals)
                {
                    if (!peripheralIds.Contains(connectedPeripheral.PeripheralId))
                    {
                        peripheralIds.Add(connectedPeripheral.PeripheralId);
                        var re = await GetRadioEntity(connectedPeripheral);
                        observer.OnNext(re);
                    }
                }

                var scanner = RadioAdapter.ScanPeripherals(RileyLinkServiceUUID)
                    .Subscribe(async peripheralResult =>
                    {
                        if (!peripheralIds.Contains(peripheralResult.RadioPeripheral.PeripheralId))
                        {
                            peripheralIds.Add(peripheralResult.RadioPeripheral.PeripheralId);
                            var re = await GetRadioEntity(peripheralResult.RadioPeripheral);
                            using(var rcr = new RadioConnectionRepository())
                            {
                                await rcr.Create(new RadioConnection
                                {
                                    RadioId = re.Id.Value,
                                    EventType = RadioConnectionEvent.Scan,
                                    Successful = true,
                                    Rssi = peripheralResult.Rssi
                                });
                            }
                            observer.OnNext(re);
                        }
                    });
                return Disposable.Create(() => { scanner.Dispose(); });
            });
        }

        private Guid? GetPeripheralId(string providerSpecificId)
        {
            Guid? retVal= null;
            if (providerSpecificId == null && providerSpecificId.Length > 3 && providerSpecificId.StartsWith("RLL"))
            {
                Guid val;
                if (Guid.TryParse(providerSpecificId.Substring(3), out val))
                {
                    retVal = val;
                }
            }
            return retVal;
        }
        public async Task<IRadioLease> GetLease(string providerSpecificId, PodRequest request, CancellationToken cancellationToken)
        {

            var peripheralId = GetPeripheralId(providerSpecificId);
            if (!peripheralId.HasValue)
                return null;

            var leaseManager = await RileyLinkLeaseManager.GetManager(RadioAdapter, peripheralId.Value);

            return await leaseManager.Acquire(request, cancellationToken);
        }

        public async Task<IRadioLease> GetIndependentLease(string providerSpecificId, CancellationToken cancellationToken)
        {
            return null;
        }

        private async Task<Radio> GetRadioEntity(IRadioPeripheral peripheral)
        {
            var rlr = new RileyLinkRadioConnection(peripheral);

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

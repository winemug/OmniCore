using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Operational;
using OmniCore.Model.Interfaces.Platform;


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

        public async Task<IRadioConnection> GetConnection(Radio radioEntity, PodRequest request, CancellationToken cancellationToken)
        {
            var peripheralLease = await RadioAdapter.LeasePeripheral(radioEntity.DeviceId, cancellationToken);
            if (peripheralLease == null)
                return null;

            return new RileyLinkRadioConnection(peripheralLease, radioEntity, request);

        }

        public IObservable<IRadio> ListRadios(CancellationToken cancellationToken)
        {
            return Observable.Create<IRadio>(async (IObserver<IRadio> observer) =>
            {
                var peripheralIds  = new HashSet<Guid>();
                var knownPeripherals = await RadioAdapter.GetKnownPeripherals(RileyLinkServiceUUID, cancellationToken);
                if (knownPeripherals != null)
                {
                    foreach (var knownPeripheral in knownPeripherals)
                    {
                        if (!peripheralIds.Contains(knownPeripheral.Id))
                        {
                            peripheralIds.Add(knownPeripheral.Id);
                            var re = await GetRadioEntity(knownPeripheral);
                            observer.OnNext(re);
                        }
                    }
                }

                var scanner = RadioAdapter.ScanPeripherals(RileyLinkServiceUUID, cancellationToken)
                    .Subscribe(async peripheralResult =>
                    {
                        if (!peripheralIds.Contains(peripheralResult.Id))
                        {
                            peripheralIds.Add(peripheralResult.Id);
                            var re = await GetRadioEntity(peripheralResult);
                            using(var rcr = RepositoryProvider.Instance.RadioConnectionRepository)
                            {
                                await rcr.Create(new RadioConnection
                                {
                                    RadioId = re.Id.Value,
                                    EventType = RadioConnectionEvent.Scan,
                                    Successful = true
                                });
                            }
                            using (var ssr = RepositoryProvider.Instance.SignalStrengthRepository)
                            {
                                await ssr.Create(new SignalStrength
                                {
                                    RadioId = re.Id.Value,
                                    ClientRadioRssi = peripheralResult.Rssi
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

        private async Task<Radio> GetRadioEntity(IRadioPeripheralScanResult peripheralScanResult)
        {
            using(var rr = RepositoryProvider.Instance.RadioRepository)
            {
                var psid = "RLL" + peripheralScanResult.Id.ToString("N");
                var entity = await rr.GetByProviderSpecificId(psid);
                if (entity == null)
                {
                    var gb = peripheralScanResult.Id.ToByteArray();
                    var macid = $"{gb[10]:X2}:{gb[11]:X2}:{gb[12]:X2}:{gb[13]:X2}:{gb[14]:X2}:{gb[15]:X2}";
                    entity = await rr.Create(new Radio
                    {
                        DeviceId = peripheralScanResult.Id,
                        DeviceIdReadable = macid,
                        DeviceName = peripheralScanResult.Name,
                        DeviceType = "RileyLink",
                        ProviderSpecificId = psid
                    });;
                }
                return entity;
            }
        }
    }
}

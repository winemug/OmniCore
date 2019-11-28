using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Constants;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Workflow;
using Unity;


namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioProvider : IRadioProvider
    {
        private readonly Guid RileyLinkServiceUUID = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        private readonly IRadioAdapter RadioAdapter;
        private readonly IRadioRepository RadioRepository;
        private readonly ISignalStrengthRepository SignalStrengthRepository;
        private readonly IUnityContainer Container;

        private readonly AsyncLock RadioDictionaryLock;
        private readonly Dictionary<Guid,IRadio> RadioDictionary;
        private readonly IRadioEventRepository RadioEventRepository;

        public RileyLinkRadioProvider(
            IRadioAdapter radioAdapter, 
            IRadioRepository radioRepository,
            ISignalStrengthRepository signalStrengthRepository,
            IRadioEventRepository radioEventRepository,
            IUnityContainer container)
        {
            RadioAdapter = radioAdapter;
            RadioRepository = radioRepository;
            SignalStrengthRepository = signalStrengthRepository;
            RadioEventRepository = radioEventRepository;
            Container = container;
            RadioDictionary = new Dictionary<Guid, IRadio>();
            RadioDictionaryLock = new AsyncLock();
        }

        public string Description => "RileyLink";

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
                        if (!peripheralIds.Contains(knownPeripheral.Uuid))
                        {
                            peripheralIds.Add(knownPeripheral.Uuid);
                            var radio = await GetRadio(knownPeripheral);
                            observer.OnNext(radio);
                        }
                    }
                }

                var scanner = RadioAdapter.ScanPeripherals(RileyLinkServiceUUID, cancellationToken)
                    .Subscribe(async peripheralResult =>
                    {
                        if (!peripheralIds.Contains(peripheralResult.Uuid))
                        {
                            peripheralIds.Add(peripheralResult.Uuid);

                            var radio = await GetRadio(peripheralResult);
                            var sse = await SignalStrengthRepository.New();
                            sse.Radio = radio.Entity;
                            sse.Rssi = peripheralResult.Rssi;
                            await SignalStrengthRepository.Create(sse);

                            var radioEvent = await RadioEventRepository.New();
                            radioEvent.Radio = radio.Entity;
                            radioEvent.EventType = RadioEvent.Scan;
                            radioEvent.Success = true;
                            await RadioEventRepository.Create(radioEvent);

                            observer.OnNext(radio);
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

        private async Task<IRadio> GetRadio(IRadioPeripheralResult peripheralResult)
        {
            using var lockObj = await RadioDictionaryLock.LockAsync();

            if (RadioDictionary.ContainsKey(peripheralResult.Uuid))
                return RadioDictionary[peripheralResult.Uuid];

            var psid = "RLL" + peripheralResult.Uuid.ToString("N");
            var entity = await RadioRepository.GetByProviderSpecificId(psid);
            if (entity == null)
            {
                var gb = peripheralResult.Uuid.ToByteArray();
                var macid = $"{gb[10]:X2}:{gb[11]:X2}:{gb[12]:X2}:{gb[13]:X2}:{gb[14]:X2}:{gb[15]:X2}";
                entity = await RadioRepository.New();
                entity.DeviceUuid = peripheralResult.Uuid;
                entity.DeviceIdReadable = macid;
                entity.DeviceName = peripheralResult.Name;
                entity.ProviderSpecificId = psid;
                await RadioRepository.Create(entity);
            }

            var radio = Container.Resolve<IRadio>(RegistrationConstants.RileyLinkRadio);
            radio.Entity = entity;
            RadioDictionary[peripheralResult.Uuid] = radio;
            return radio;
        }
    }
}

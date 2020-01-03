using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Constants;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Services;


namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioServiceBase : OmniCoreServiceBase, IRadioService
    {
        private readonly Guid RileyLinkServiceUUID = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        private readonly IRadioAdapter RadioAdapter;
        private readonly IRadioRepository RadioRepository;
        private readonly ISignalStrengthRepository SignalStrengthRepository;
        private readonly ICoreContainer<IServerResolvable> Container;

        private readonly AsyncLock RadioDictionaryLock;
        private readonly Dictionary<Guid,IRadio> RadioDictionary;
        private readonly IRadioEventRepository RadioEventRepository;

        public RileyLinkRadioServiceBase(
            IRadioAdapter radioAdapter, 
            IRadioRepository radioRepository,
            IRadioEventRepository radioEventRepository,
            ICoreContainer<IServerResolvable> container) : base()
        {
            RadioAdapter = radioAdapter;
            RadioRepository = radioRepository;
            RadioEventRepository = radioEventRepository;
            Container = container;
            RadioDictionary = new Dictionary<Guid, IRadio>();
            RadioDictionaryLock = new AsyncLock();
        }

        public string Description => "RileyLink";
       
        public IObservable<IRadio> ListRadios()
        {
            return Observable.Create<IRadio>( (IObserver<IRadio> observer) =>
            {
                var cts = new CancellationTokenSource();
                
                var scanner = RadioAdapter.FindPeripherals(RileyLinkServiceUUID)
                    .Subscribe(async peripheralResult =>
                    {
                        var radio = await GetRadio(peripheralResult.Peripheral, cts.Token);
                        var radioEvent = RadioEventRepository.New();
                        radioEvent.Radio = radio.Entity;
                        radioEvent.EventType = RadioEvent.Scan;
                        radioEvent.Success = true;
                        await RadioEventRepository.Create(radioEvent, cts.Token);

                        observer.OnNext(radio);
                    });
                
                return Disposable.Create(() =>
                {
                    cts.Cancel();
                    cts.Dispose();
                    scanner.Dispose();
                });
            });
        }

        private async Task<IRadio> GetRadio(IRadioPeripheral peripheral, CancellationToken cancellationToken)
        {
            using var lockObj = await RadioDictionaryLock.LockAsync();
            IRadio radio = null;
            
            if (RadioDictionary.ContainsKey(peripheral.Uuid))
            {
                radio = RadioDictionary[peripheral.Uuid];
            }
            else
            {
                var entity = await RadioRepository.ByDeviceUuid(peripheral.Uuid);
                if (entity == null)
                {
                    entity = RadioRepository.New();
                    entity.DeviceUuid = peripheral.Uuid;
                    entity.DeviceName = peripheral.Name;
                    await RadioRepository.Create(entity, cancellationToken);
                }

                radio = Container.Get<IRadio>();
                radio.Entity = entity;
                radio.Peripheral = peripheral;
                radio.Peripheral.RssiUpdateTimeSpan = radio.GetConfiguration().RssiUpdateInterval;

                RadioDictionary.Add(peripheral.Uuid, radio);
            }
            radio.Peripheral = peripheral;
            return radio;
        }

        protected override Task OnStart(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnPause(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task OnResume(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

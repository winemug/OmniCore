using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Constants;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Services;


namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioService : OmniCoreServiceBase, IRadioService
    {
        private readonly Guid RileyLinkServiceUUID = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        private readonly IRadioAdapter RadioAdapter;
        private readonly IRadioRepository RadioRepository;
        private readonly ICoreContainer<IServerResolvable> Container;

        private readonly AsyncLock RadioDictionaryLock;
        private readonly Dictionary<Guid,IRadio> RadioDictionary;
        private readonly IRadioEventRepository RadioEventRepository;

        public RileyLinkRadioService(
            IRadioAdapter radioAdapter, 
            IRadioRepository radioRepository,
            ICoreContainer<IServerResolvable> container) : base()
        {
            RadioAdapter = radioAdapter;
            RadioRepository = radioRepository;
            Container = container;
            RadioDictionary = new Dictionary<Guid, IRadio>();
            RadioDictionaryLock = new AsyncLock();
        }

        public string Description => "RileyLink";

        public IObservable<IRadioPeripheral> ScanRadios()
        {
            return Observable.Create<IRadioPeripheral>( async (IObserver<IRadioPeripheral> observer) =>
            {
                var cts = new CancellationTokenSource();

                var knownRadioIds = (await RadioRepository.All(cts.Token)).Select(r => r.DeviceUuid);
                
                var scanner = RadioAdapter.FindPeripherals()
                    //.Where(p => p.ServiceUuids.Contains(RileyLinkServiceUUID))
                    .Where(p => !knownRadioIds.Contains(p.PeripheralUuid))
                    .Subscribe(async peripheral =>
                    {
                        observer.OnNext(peripheral);
                    });
                
                return Disposable.Create(() =>
                {
                    cts.Cancel();
                    cts.Dispose();
                    scanner.Dispose();
                });
            });
        }

        public Task<bool> VerifyPeripheral(IRadioPeripheral peripheral)
        {
            throw new NotImplementedException();
        }

        public IObservable<IRadio> ListRadios()
        {
            return Observable.Create<IRadio>( async (IObserver<IRadio> observer) =>
            {
                var cts = new CancellationTokenSource();

                foreach (var radioEntity in await RadioRepository.All(cts.Token))
                {
                    observer.OnNext(await GetRadio(radioEntity, cts.Token));
                }

                observer.OnCompleted();

                return Disposable.Create(() =>
                {
                    cts.Cancel();
                    cts.Dispose();
                });
            });
        }

        private async Task<IRadio> GetRadio(IRadioEntity radioEntity, CancellationToken cancellationToken)
        {
            using var lockObj = await RadioDictionaryLock.LockAsync();
            IRadio radio = null;
            
            if (RadioDictionary.ContainsKey(radioEntity.DeviceUuid))
            {
                radio = RadioDictionary[radioEntity.DeviceUuid];
            }
            else
            {
                radio = Container.Get<IRadio>();
                radio.Start(radioEntity, RadioAdapter.GetPeripheral(radioEntity.DeviceUuid));

                RadioDictionary.Add(radioEntity.DeviceUuid, radio);
            }
            return radio;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            foreach (var radioEntity in await RadioRepository.All(cancellationToken))
            {
                await GetRadio(radioEntity, cancellationToken);
            }
        }

        protected override async Task OnStop(CancellationToken cancellationToken)
        {
            using var lockObj = await RadioDictionaryLock.LockAsync();
            var disconnectTasks = new List<Task>();
            foreach (var radio in RadioDictionary.Values)
            {
                disconnectTasks.Add(Task.Run(async () =>
                {
                    using var _ = await radio.Peripheral.Lease(cancellationToken);
                    await radio.Peripheral.Disconnect(cancellationToken);
                    radio.Dispose();
                }, cancellationToken));
            }
            await Task.WhenAll(disconnectTasks);
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

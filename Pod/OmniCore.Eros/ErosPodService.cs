﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Nito.AsyncEx;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fody;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform;
using Unity;

namespace OmniCore.Eros
{
    public class ErosPodService : IPodService
    {
        public string Description => "Omnipod Eros";

        public IRadioService[] RadioProviders { get; }
        
        private readonly IRadioAdapter RadioAdapter;
        private readonly IUnityContainer Container;

        private readonly Dictionary<long, IPod> PodDictionary;
        private readonly AsyncLock PodLock;
        private readonly IPodRepository PodRepository;


        public ErosPodService(
            IRadioAdapter radioAdapter,
            IPodRepository podRepository,
            IUnityContainer container,
            [Dependency(RegistrationConstants.RileyLink)] IRadioService radioServiceRileyLink)
        {
            RadioProviders = new[] {radioServiceRileyLink};
            RadioAdapter = radioAdapter;
            Container = container;
            PodRepository = podRepository;
            PodDictionary = new Dictionary<long, IPod>();
            PodLock = new AsyncLock();
        }

        public async Task Startup(CancellationToken cancellationToken)
        {
            await foreach (var activePodEntity in PodRepository.ActivePods())
            {
                await GetPodInternal(activePodEntity);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public async Task Shutdown(CancellationToken cancellationToken)
        {
            using (var podLock = await PodLock.LockAsync())
            {
                foreach(var pod in PodDictionary.Values)
                {
                    await pod.StopQueue();
                }
            }
        }

        public async IAsyncEnumerable<IPod> ActivePods()
        {
            await foreach (var activePodEntity in PodRepository.ActivePods())
            {
                yield return await GetPodInternal(activePodEntity);
            }
        }
        
        public async IAsyncEnumerable<IPod> ArchivedPods()
        {
            await foreach (var archivedPodEntity in PodRepository.ActivePods())
            {
                var pod = Container.Resolve<IPod>(RegistrationConstants.OmnipodEros);
                pod.Entity = archivedPodEntity;
                yield return pod;
            }
        }

//        public async Task Archive(Pod pod)
//        {
//            using(var pr = RepositoryProvider.Instance.PodRepository)
//            {
//                pod.Archived = true;
//                await pr.CreateOrUpdate(pod);
//            }
//        }
//        public async Task<Pod> Register(Pod pod, UserProfile up, List<Radio> radios)
//        {
//            using(var pr = RepositoryProvider.Instance.PodRepository)
//            {
//                if (!pod.PodUniqueId.HasValue)
//                    pod.PodUniqueId = Guid.NewGuid();
//                pod.RadioIds = radios.Select(r => r.Id.Value).ToArray();
//                return await pr.CreateOrUpdate(pod);
//            }
//        }
//
//        public IObservable<Radio> ListRadios()
//        {
//            return Observable.Create<Radio>(async (IObserver<Radio> observer) =>
//            {
//                var cts = new CancellationTokenSource();
//                var ct = cts.Token;
//                var radioids = new ConcurrentDictionary<string,int>();
//                var disposables = new List<IDisposable>();
//                foreach (var radioProvider in _radioProviders)
//                {
//                    disposables.Add(radioProvider.ListRadios(ct)
//                        .Subscribe(radio =>
//                        {
//                            int unused;
//                            if (!radioids.TryGetValue(radio.ProviderSpecificId, out unused))
//                            {
//                                radioids.TryAdd(radio.ProviderSpecificId, 0);
//                                observer.OnNext(radio);
//                            }
//                        })) ;
//                }
//
//                return Disposable.Create(() =>
//                {
//                    cts.Cancel();
//                    foreach(var disposable in disposables)
//                        disposable.Dispose();
//                    cts.Dispose();
//                });
//            });
//        }

        private uint GenerateRadioAddress()
        {
            var random = new Random();
            var buffer = new byte[3];
            random.NextBytes(buffer);
            uint address = 0x34000000;
            address |= (uint)buffer[0] << 16;
            address |= (uint)buffer[1] << 8;
            address |= (uint)buffer[2];
            return address;
        }

//        private async Task<ErosRequestProcessor> GetProcessor(long podId)
//        {
//            if (!_requestProcessors.ContainsKey(podId))
//            {
//                using(var pr = RepositoryProvider.Instance.PodRepository)
//                {
//                    var pod = await pr.Read(podId);
//
//                    var erp = new ErosRequestProcessor();
//                    await erp.Initialize(pod, _backgroundTaskFactory, _radioProviders);
//                    _requestProcessors.Add(podId, erp);
//                }
//            }
//            return _requestProcessors[podId];
//        }
//
//        public async Task QueueRequest(PodRequest request)
//        {
//            try
//            {
//                var processor = await GetProcessor(request.PodId);
//                await processor.QueueRequest(request);
//            }
//            catch  { throw; }
//
//        }
//
//        public async Task<bool> WaitForResult(PodRequest request, int timeout)
//        {
//            var processor = await GetProcessor(request.PodId);
//            return await processor.WaitForResult(request, 0);
//        }
//
//        public async Task<bool> CancelRequest(PodRequest request)
//        {
//            var processor = await GetProcessor(request.PodId);
//            return await processor.CancelRequest(request);
//        }
//
//        public async Task<List<PodRequest>> GetActiveRequests(Pod pod)
//        {
//            var processor = await GetProcessor(pod.Id.Value);
//            return await processor.GetActiveRequests();
//        }
//
//        public async Task<List<PodRequest>> GetActiveRequests()
//        {
//            var processors = new List<ErosRequestProcessor>();
//            var list = new List<PodRequest>();
//            lock(_requestProcessors)
//            {
//                foreach(var processor in _requestProcessors.Values)
//                    processors.Add(processor);
//            }
//
//            foreach(var processor in processors)
//                list.AddRange(await processor.GetActiveRequests());
//
//            return list;
//        }

        public async Task<IPod> New(IUserEntity user, IMedicationEntity medication, IList<IRadioEntity> radios)
        {
            var podEntity = PodRepository.New();
            podEntity.Medication = medication;
            podEntity.User = user;
            podEntity.Radios = radios;
            podEntity.UniqueId = Guid.NewGuid();
            podEntity.RadioAddress = GenerateRadioAddress();
            await PodRepository.Create(podEntity, CancellationToken.None);
            return await GetPodInternal(podEntity);
        }

        public async Task<IPod> Register(IPodEntity podEntity, IUserEntity user, IList<IRadioEntity> radios)
        {
            throw new NotImplementedException();
        }

        private async Task<IPod> GetPodInternal(IPodEntity podEntity)
        {
            using var podLock = await PodLock.LockAsync();
            if (PodDictionary.ContainsKey(podEntity.Id))
                return PodDictionary[podEntity.Id];

            var pod = Container.Resolve<IPod>(RegistrationConstants.OmnipodEros);
            pod.Entity = podEntity;
            await pod.StartQueue();
            PodDictionary[podEntity.Id] = pod;
            return pod;
        }
    }
}

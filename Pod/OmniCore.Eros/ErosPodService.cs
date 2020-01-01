using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Nito.AsyncEx;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Services;

namespace OmniCore.Eros
{
    public class ErosPodService : OmniCoreService, IPodService
    {
        public string Description => "Omnipod Eros";

        public IRadioService[] RadioProviders { get; }
        
        private readonly IRadioAdapter RadioAdapter;
        private readonly ICoreServices Services;

        private readonly ConcurrentDictionary<long, IPod> PodDictionary;
        private readonly AsyncLock PodCreateLock;
        private readonly IPodRepository PodRepository;


        public ErosPodService(
            ICoreServices services,
            ICoreApplicationService applicationService,
            IRadioAdapter radioAdapter,
            IPodRepository podRepository,
            IRadioService radioServiceRileyLink)
        {
            Services = services;
            RadioProviders = new[] {radioServiceRileyLink};
            RadioAdapter = radioAdapter;
            PodRepository = podRepository;
            PodDictionary = new ConcurrentDictionary<long, IPod>();
            PodCreateLock = new AsyncLock();
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
                var pod = Services.Container.Get<IPod>();
                pod.Entity = archivedPodEntity;
                yield return pod;
            }
        }

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
            using var podLock = await PodCreateLock.LockAsync();
            if (PodDictionary.ContainsKey(podEntity.Id))
                return PodDictionary[podEntity.Id];

            var pod = Services.Container.Get<IPod>();
            pod.Entity = podEntity;
            await pod.StartQueue();
            PodDictionary[podEntity.Id] = pod;
            return pod;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            var previousState = this.Services.ApplicationService.ReadPreferences(new []
            {
                ("ErosPodService_StopRequested_ActiveRequests", string.Empty),
            })[0];

            if (!string.IsNullOrEmpty(previousState.Value))
            {
                //TODO: check states of requests - create notifications
                
                Services.ApplicationService.StorePreferences(new []
                {
                    ("ErosPodService_StopRequested_ActiveRequests", string.Empty),
                });
            }
            
            Services.OnUnexpectedStopRequest.Subscribe((services) =>
            {
                var listCopy = PodDictionary.Values.ToList();
                
                var runningRequestIds = new StringBuilder();
                foreach (var pod in listCopy)
                {
                    var ar = pod.ActiveRequest;
                    if (ar != null)
                    {
                        runningRequestIds.Append($"{ar.Entity.Id},");
                        if (ar.CanCancel)
                            ar.RequestCancellation();
                    }
                }
                services.ApplicationService.StorePreferences(new []
                {
                    ("ErosPodService_StopRequested_ActiveRequests", runningRequestIds.ToString()),
                });
            });
            
            await foreach (var activePodEntity in PodRepository.ActivePods())
            {
                await GetPodInternal(activePodEntity);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        protected override async Task OnStop(CancellationToken cancellationToken)
        {
            using (var podLock = await PodCreateLock.LockAsync())
            {
                foreach (var pod in PodDictionary.Values)
                {
                    await pod.StopQueue();
                }
            }
            PodDictionary.Clear();
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

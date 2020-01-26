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
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Constants;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Services;

namespace OmniCore.Eros
{
    public class ErosPodService : OmniCoreServiceBase, IPodService
    {
        public string Description => "Omnipod Eros";

        public IRadioService[] RadioProviders { get; }
        
        private readonly IRadioAdapter RadioAdapter;
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly ICoreApi Api;

        private readonly ConcurrentDictionary<long, IPod> PodDictionary;
        private readonly AsyncLock PodCreateLock;


        public ErosPodService(
            IRadioAdapter radioAdapter,
            IRadioService radioServiceRileyLink,
            ICoreContainer<IServerResolvable> container,
            ICoreApplicationFunctions applicationFunctions,
            ICoreApi api)
        {
            Container = container;
            ApplicationFunctions = applicationFunctions;
            Api = api;

            RadioProviders = new[] {radioServiceRileyLink};
            RadioAdapter = radioAdapter;
            PodDictionary = new ConcurrentDictionary<long, IPod>();
            PodCreateLock = new AsyncLock();
        }

        public IList<IPod> ActivePods(CancellationToken cancellationToken)
        {
            using var context = Container.Get<IRepositoryContext>();
            var pods = new List<IPod>();
            context.Pods.Where(p => !p.IsDeleted)
                .Include(p => p.Medication)
                .Include(p => p.Radio)
                .Include(p => p.User)
                .Include(p => p.ExpiredReminder)
                .Include(p => p.ExpiresSoonReminder)
                .Include(p => p.ReservoirLowReminder)
                .ToList()
                .ForEach(async p => pods.Add(await GetPodInternal(p)));
            return pods;
        }
        
        public IList<IPod> ArchivedPods(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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

        public async Task<IPod> New(UserEntity user, MedicationEntity medication, RadioEntity radio)
        {
            var podEntity = new PodEntity
            {
                Medication = medication,
                User = user,
                Radio = radio,
                RadioAddress = GenerateRadioAddress()
            };

            using var context = Container.Get<IRepositoryContext>();
            context.Pods.Add(podEntity);
            await context.Save(CancellationToken.None);

            return await GetPodInternal(podEntity);
        }

        public Task<IPod> Register(PodEntity podEntity, UserEntity user, RadioEntity radio)
        {
            throw new NotImplementedException();
        }

        private async Task<IPod> GetPodInternal(PodEntity podEntity)
        {
            using var podLock = await PodCreateLock.LockAsync();
            if (PodDictionary.ContainsKey(podEntity.Id))
                return PodDictionary[podEntity.Id];

            var pod = Container.Get<IPod>();
            pod.Entity = podEntity;
            PodDictionary[podEntity.Id] = pod;
            return pod;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            var previousState = ApplicationFunctions.ReadPreferences(new []
            {
                ("ErosPodService_StopRequested_ActiveRequests", string.Empty),
            })[0];

            if (!string.IsNullOrEmpty(previousState.Value))
            {
                //TODO: check states of requests - create notifications
                
                ApplicationFunctions.StorePreferences(new []
                {
                    ("ErosPodService_StopRequested_ActiveRequests", string.Empty),
                });
            }

            foreach (var pod in ActivePods(cancellationToken))
            {
                await pod.StartMonitoring();
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public override Task OnBeforeStopRequest()
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
            ApplicationFunctions.StorePreferences(new []
            {
                ("ErosPodService_StopRequested_ActiveRequests", runningRequestIds.ToString()),
            });
            return Task.CompletedTask;
        }

        protected override async Task OnStop(CancellationToken cancellationToken)
        {
            using (var podLock = await PodCreateLock.LockAsync())
            {
                foreach (var pod in PodDictionary.Values)
                {
                    pod.Dispose();
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

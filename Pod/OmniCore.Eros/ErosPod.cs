using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using IErosRadio = OmniCore.Model.Interfaces.Services.IErosRadio;

namespace OmniCore.Eros
{
    public class ErosPod : IErosPod
    {
        private readonly IContainer Container;
        private readonly IPodService PodService;
        private readonly IRepositoryService RepositoryService;
        private readonly ISubject<IEnumerable<IErosRadio>> RadiosUpdatedSubject;
        private readonly ISubject<IPod> PodArchivedSubject;

        public ErosPod(IContainer container,
            IRepositoryService repositoryService,
            IPodService podService)
        {
            RepositoryService = repositoryService;
            PodService = podService;
            Container = container;
            RunningState = new PodRunningState();
            RadiosUpdatedSubject = new Subject<IEnumerable<IErosRadio>>();
            PodArchivedSubject = new Subject<IPod>();
        }
        public PodEntity Entity { get; set; }
        public PodRunningState RunningState { get; }

        public async Task Archive(CancellationToken cancellationToken)
        {
            using (var context =
                await RepositoryService.GetContextReadWrite(cancellationToken))
            {
                context.WithExisting(Entity)
                    .WithExisting(Entity.Medication)
                    .WithExisting(Entity.User)
                    .WithExisting(Entity.PodRadios);

                Entity.IsDeleted = true;

                await context.Save(cancellationToken);
            }
            PodArchivedSubject.OnNext(this);
        }

        public Task<IList<IPodTask>> GetActiveRequests()
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<IErosRadio>> WhenRadiosUpdated() => RadiosUpdatedSubject.AsObservable();

        public IObservable<IPod> WhenPodArchived() => PodArchivedSubject.AsObservable();

        public async Task UpdateRadioList(IEnumerable<IErosRadio> radios, CancellationToken cancellationToken)
        {
            using (var context = 
                await RepositoryService.GetContextReadWrite(cancellationToken))
            {
                context.WithExisting(Entity)
                    .WithExisting(Entity.Medication)
                    .WithExisting(Entity.User)
                    .WithExisting(Entity.PodRadios);

                Entity.PodRadios.Clear();

                foreach (var radio in radios)
                {
                    Entity.PodRadios.Add(new PodRadioEntity
                    {
                        Pod = Entity,
                        Radio = radio.Entity
                    });
                }
                await context.Save(cancellationToken);
            }

            RadiosUpdatedSubject.OnNext(radios);
        }

        public Task<IPodTask> Activate(IErosRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IPodTask> Acquire(IErosRadio radio, CancellationToken cancellationToken)
        {
            var request = (await Container.Get<IErosPodRequest>())
                .WithPod(this)
                .WithAcquireRequest();

            var task = (await Container.Get<IPodTask>())
                .WithRequest(request);

            return task;
        }

        public Task<IPodTask> VerifyIdentity(uint lotNumber, uint serialNumber, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> Start()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodTask> Status(StatusRequestType requestType)
        {
            var request = (await Container.Get<IErosPodRequest>())
                .WithPod(this)
                .WithStatusRequest(requestType);

            var task = (await Container.Get<IPodTask>())
                .WithRequest(request);

            return task;
        }

        public Task<IPodTask> ConfigureAlerts()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> AcknowledgeAlerts()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> SetBasalSchedule()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> CancelBasal()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> SetTempBasal()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> CancelTempBasal()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> Bolus()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> CancelBolus()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> StartExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> CancelExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public Task<IPodTask> Deactivate()
        {
            throw new NotImplementedException();
        }

        public void StartMonitoring()
        {
        }

        public void Dispose()
        {
            // RequestQueue.Shutdown();
        }

        private async Task StartStateMonitoring()
        {
            using var context = await RepositoryService.GetContextReadOnly(CancellationToken.None);
            var responses = context.PodRequests
                .Where(pr => pr.Pod.Id == Entity.Id)
                .OrderByDescending(p => p.Created)
                .Include(pr => pr.Responses)
                .SelectMany(pr => pr.Responses)
                .OrderByDescending(r => r.Created);

            RunningState.LastRadioContact = responses.FirstOrDefault()?.Created;
            RunningState.State = DetermineRunningState(responses);

            RunningState.LastUpdated = DateTimeOffset.UtcNow;
        }

        private PodState DetermineRunningState(IOrderedQueryable<PodResponseEntity> responses)
        {
            var state = PodState.Unknown;
            var progress = responses
                .FirstOrDefault(r => r.Progress.HasValue)?
                .Progress;

            switch (progress)
            {
                case PodProgress.InitialState:
                case PodProgress.TankPowerActivated:
                case PodProgress.TankFillCompleted:
                    state = PodState.Pairing;
                    break;
                case PodProgress.PairingSuccess:
                    state = PodState.Paired;
                    break;
                case PodProgress.Purging:
                    state = PodState.Priming;
                    break;
                case PodProgress.ReadyForInjection:
                    state = PodState.Primed;
                    break;
                case PodProgress.BasalScheduleSet:
                case PodProgress.Priming:
                    state = PodState.Starting;
                    break;
                case PodProgress.Running:
                case PodProgress.RunningLow:
                    state = PodState.Started;
                    break;
                case PodProgress.ErrorShuttingDown:
                    state = PodState.Faulted;
                    break;
                case PodProgress.AlertExpiredShuttingDown:
                    state = PodState.Expired;
                    break;
                case PodProgress.Inactive:
                    state = PodState.Stopped;
                    break;
            }

            return state;
        }

        private uint GenerateRadioAddress()
        {
            var random = new Random();
            var buffer = new byte[3];
            random.NextBytes(buffer);
            uint address = 0x34000000;
            address |= (uint) buffer[0] << 16;
            address |= (uint) buffer[1] << 8;
            address |= buffer[2];
            return address;
        }
    }
}
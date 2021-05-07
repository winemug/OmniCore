using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Interfaces.Services.Requests;
using AsyncLock = Nito.AsyncEx.AsyncLock;
using IErosRadio = OmniCore.Model.Interfaces.Services.IErosRadio;
using ILogger = OmniCore.Model.Interfaces.Services.ILogger;

namespace OmniCore.Eros
{
    public class ErosPod : IErosPod
    {
        public PodEntity Entity { get; private set; }
        public PodRunningState RunningState { get; }
        public IObservable<IPod> WhenPodArchived() => PodArchivedSubject.AsObservable();

        private readonly IContainer Container;
        private readonly IPodService PodService;
        private readonly ILogger Logger;
        private readonly IRepositoryService RepositoryService;
        private readonly ISubject<IPod> PodArchivedSubject;
        private readonly AsyncLock ProbeStartStopLock;

        private CancellationTokenSource StatusCheckCancellationTokenSource;
        private IDisposable StatusCheckSubscription;
        public ErosPodRequestQueue RequestQueue { get; private set; }
        public ErosPodConversationHandler ConversationHandler { get; private set; }
        
        public ErosPod(IContainer container,
            IRepositoryService repositoryService,
            IPodService podService,
            ILogger logger)
        {
            RepositoryService = repositoryService;
            PodService = podService;
            Container = container;
            Logger = logger;
            RunningState = new PodRunningState();
            PodArchivedSubject = new Subject<IPod>();
            ProbeStartStopLock = new AsyncLock();
        }
        
        public async Task Initialize(PodEntity podEntity, CancellationToken cancellationToken)
        {
            Entity = podEntity;
            RequestQueue = await Container.Get<ErosPodRequestQueue>();
            ConversationHandler = await Container.Get<ErosPodConversationHandler>();
            await RequestQueue.Initialize(this, cancellationToken);
        }

        public async Task Archive(CancellationToken cancellationToken)
        {
            if (Entity.IsDeleted)
                return;

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

        public Task<IList<IPodRequest>> GetActiveRequests()
        {
            throw new NotImplementedException();
        }

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

            await RequestQueue.Initialize(this, cancellationToken);
        }

        public async Task AsPaired(uint radioAddress, uint lotNumber, uint serialNumber,
            CancellationToken cancellationToken)
        {
            using var context =
                await RepositoryService.GetContextReadWrite(cancellationToken);
            context.WithExisting(Entity)
                .WithExisting(Entity.Medication)
                .WithExisting(Entity.User)
                .WithExisting(Entity.PodRadios);

            Entity.RadioAddress = radioAddress;
            Entity.Lot = lotNumber;
            Entity.Serial = serialNumber;
            await context.Save(cancellationToken);
        }

        public async Task<IPodRequest> DebugAction(CancellationToken cancellationToken)
        {
            var sr = await Container.Get<ErosPodStatusRequest>();
            sr.ForPod(this);
            return await sr
                .WithUpdateStatus()
                .Submit(cancellationToken);
        }

        public async Task<IPodActivationRequest> ActivationRequest()
        {
            return (IPodActivationRequest) (await Container.Get<ErosPodActivationRequest>())
                .ForPod(this);
        }

        public async Task<IPodBolusRequest> BolusRequest()
        {
            return (IPodBolusRequest) (await Container.Get<ErosPodBolusRequest>())
                .ForPod(this);
        }

        public async Task<IPodDeliveryCancellationRequest> CancellationRequest()
        {
            return (IPodDeliveryCancellationRequest) (await Container.Get<ErosPodDeliveryCancellationRequest>())
                .ForPod(this);
        }

        public async Task<IPodScheduledDeliveryRequest> ScheduledDeliveryRequest()
        {
            return (IPodScheduledDeliveryRequest) (await Container.Get<ErosPodScheduledDeliveryRequest>())
                .ForPod(this);
        }
        public async Task SetNextMessageSequence(int nextSequence, CancellationToken cancellationToken)
        {
            using var context =
                await RepositoryService.GetContextReadWrite(cancellationToken);
            context.WithExisting(Entity)
                .WithExisting(Entity.Medication)
                .WithExisting(Entity.User)
                .WithExisting(Entity.PodRadios);
            Entity.NextMessageSequence = nextSequence;
            await context.Save(cancellationToken);
        }
        public void Dispose()
        {
            RequestQueue.Shutdown();
        }

        // private async Task UpdateRunningState()
        // {
        //     using var context = await RepositoryService.GetContextReadOnly(CancellationToken.None);
        //     var responses = context.PodRequests
        //         .Where(pr => pr.Pod.Id == Entity.Id)
        //         .OrderByDescending(p => p.Created)
        //         .Include(pr => pr.Responses)
        //         .SelectMany(pr => pr.Responses)
        //         .OrderByDescending(r => r.Created);
        //
        //     RunningState.LastRadioContact = responses.FirstOrDefault()?.Created;
        //     RunningState.State = DetermineRunningState(responses);
        //
        //     RunningState.LastUpdated = DateTimeOffset.UtcNow;
        // }
        //
        // private PodState DetermineRunningState(IOrderedQueryable<PodResponseEntity> responses)
        // {
        //     var state = PodState.Unknown;
        //     var progress = responses
        //         .FirstOrDefault(r => r.Progress.HasValue)?
        //         .Progress;
        //
        //     switch (progress)
        //     {
        //         case PodProgress.InitialState:
        //         case PodProgress.TankPowerActivated:
        //         case PodProgress.TankFillCompleted:
        //             state = PodState.Pairing;
        //             break;
        //         case PodProgress.PairingSuccess:
        //             state = PodState.Paired;
        //             break;
        //         case PodProgress.Purging:
        //             state = PodState.Priming;
        //             break;
        //         case PodProgress.ReadyForInjection:
        //             state = PodState.Primed;
        //             break;
        //         case PodProgress.BasalScheduleSet:
        //         case PodProgress.Priming:
        //             state = PodState.Starting;
        //             break;
        //         case PodProgress.Running:
        //         case PodProgress.RunningLow:
        //             state = PodState.Started;
        //             break;
        //         case PodProgress.ErrorShuttingDown:
        //             state = PodState.Faulted;
        //             break;
        //         case PodProgress.AlertExpiredShuttingDown:
        //             state = PodState.Expired;
        //             break;
        //         case PodProgress.Inactive:
        //             state = PodState.Stopped;
        //             break;
        //     }
        //
        //     return state;
        // }

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
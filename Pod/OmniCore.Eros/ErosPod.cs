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
using AsyncLock = Nito.AsyncEx.AsyncLock;
using IErosRadio = OmniCore.Model.Interfaces.Services.IErosRadio;
using ILogger = OmniCore.Model.Interfaces.Services.ILogger;

namespace OmniCore.Eros
{
    public class ErosPod : IErosPod
    {
        private readonly IContainer Container;
        private readonly IPodService PodService;
        private readonly ILogger Logger;
        private readonly IRepositoryService RepositoryService;
        private readonly ISubject<IPod> PodArchivedSubject;
        private readonly AsyncLock ProbeStartStopLock;
        private readonly IErosRadioProvider[] ErosRadioProviders;

        private ErosRadioSelector RadioSelector;
        public PodEntity Entity { get; private set; }
        public PodRunningState RunningState { get; }
        public IObservable<IPod> WhenPodArchived() => PodArchivedSubject.AsObservable();

        private CancellationTokenSource StatusCheckCancellationTokenSource;
        private IDisposable StatusCheckSubscription;
        private IErosRadio LastGoodRadio = null;
        

        public ErosPod(IContainer container,
            IRepositoryService repositoryService,
            IPodService podService,
            ILogger logger,
            IErosRadioProvider[] erosRadioProviders)
        {
            RepositoryService = repositoryService;
            PodService = podService;
            Container = container;
            RunningState = new PodRunningState();
            PodArchivedSubject = new Subject<IPod>();
            ProbeStartStopLock = new AsyncLock();
            ErosRadioProviders = erosRadioProviders;
        }
        
        public async Task Initialize(PodEntity podEntity, CancellationToken cancellationToken)
        {
            Entity = podEntity;
            RadioSelector = await GetRadioSelector(cancellationToken);
            await StartProbing(TimeSpan.FromSeconds(15), cancellationToken);
        }

        public async Task Archive(CancellationToken cancellationToken)
        {
            StopProbing(cancellationToken);
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

        public async Task UpdateRadioList(IEnumerable<IErosRadio> radios, CancellationToken cancellationToken)
        {
            StopProbing(cancellationToken);
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

            RadioSelector = await GetRadioSelector(cancellationToken);
            await StartProbing(cancellationToken);
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

        public void Dispose()
        {
            // RequestQueue.Shutdown();
            StopProbing(CancellationToken.None);
        }

        private async Task UpdateRunningState()
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
        
        private async Task<ErosRadioSelector> GetRadioSelector(CancellationToken cancellationToken)
        {
            var radios = new List<IErosRadio>();
            Entity
                .PodRadios
                .Select(pr => pr.Radio)
                .ToList()
                .ForEach(async r =>
                {
                    radios.Add(await 
                        ErosRadioProviders.Single(rp => rp.ServiceUuid == r.ServiceUuid)
                            .GetRadio(r.DeviceUuid, cancellationToken));
                });

            var selector = await Container.Get<ErosRadioSelector>();
            await selector.Initialize(radios);
            return selector;
        }
        
                private async Task StartProbing(CancellationToken cancellationToken)
        {
            await StartProbing(Entity.Options.StatusCheckIntervalGood, cancellationToken);
        }
        
        private async Task StartProbing(TimeSpan initialProbe, CancellationToken cancellationToken)
        {
            if (!Entity.IsDeleted)
            {
                await ScheduleProbe(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
        private async Task ScheduleProbe(TimeSpan interval, CancellationToken cancellationToken)
        {
            using var _ = await ProbeStartStopLock.LockAsync(cancellationToken);
            StatusCheckCancellationTokenSource?.Dispose();
            StatusCheckCancellationTokenSource = new CancellationTokenSource();

            StatusCheckSubscription?.Dispose();
            StatusCheckSubscription = NewThreadScheduler.Default.Schedule(
                interval,
                async () =>
                {
                    var nextInterval = Entity.Options.StatusCheckIntervalGood;
                    try
                    {
                        nextInterval = await PerformProbe(StatusCheckCancellationTokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        if (StatusCheckCancellationTokenSource.IsCancellationRequested)
                        {
                            Logger.Information($"Pod probe canceled");
                        }
                        else
                        {
                            Logger.Warning($"Pod probe failed", e);
                            nextInterval = Entity.Options.StatusCheckIntervalBad;
                        }
                    }
#if DEBUG
                    nextInterval = TimeSpan.FromSeconds(10);
#endif

                    await ScheduleProbe(nextInterval, StatusCheckCancellationTokenSource.Token);
                });
        }

        private void StopProbing(CancellationToken cancellationToken)
        {
            using var _ = ProbeStartStopLock.Lock(cancellationToken);
            StatusCheckSubscription?.Dispose();
            StatusCheckSubscription = null;

            if (StatusCheckCancellationTokenSource != null)
            {
                StatusCheckCancellationTokenSource.Cancel();
                StatusCheckCancellationTokenSource.Dispose();
                StatusCheckCancellationTokenSource = null;
            }
        }

        private async Task<TimeSpan> PerformProbe(CancellationToken cancellationToken)
        {
            Logger.Information("Starting pod probe");

            var radio = await RadioSelector.Select();
            await radio.PerformHealthCheck(cancellationToken);
            
            Logger.Information("Pod probe ended");
            return Entity.Options.StatusCheckIntervalGood;
        }
    }
}
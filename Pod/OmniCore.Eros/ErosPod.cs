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
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using IErosRadio = OmniCore.Model.Interfaces.Services.Facade.IErosRadio;

namespace OmniCore.Eros
{
    public class ErosPod : IErosPod
    {
        private readonly IContainer<IServiceInstance> Container;
        private readonly IPodService PodService;
        private readonly IRepositoryService RepositoryService;
        private readonly ErosRequestQueue RequestQueue;
        private readonly ISubject<IEnumerable<IErosRadio>> RadiosUpdatedSubject;

        public ErosPod(IContainer<IServiceInstance> container,
            IRepositoryService repositoryService,
            IPodService podService,
            ErosRequestQueue requestQueue)
        {
            RepositoryService = repositoryService;
            PodService = podService;
            Container = container;
            RequestQueue = requestQueue;
            RunningState = new PodRunningState();
            RadiosUpdatedSubject = new Subject<IEnumerable<IErosRadio>>();
        }
        public PodEntity Entity { get; set; }
        public PodRunningState RunningState { get; }

        public Task Archive()
        {
            throw new NotImplementedException();
        }

        public Task<IList<IPodRequest>> GetActiveRequests()
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<IErosRadio>> WhenRadiosUpdated() => RadiosUpdatedSubject.AsObservable();

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
                context.Save(cancellationToken);
            }

            //TODO
            RadiosUpdatedSubject.OnNext(radios);
        }

        public Task<IPodRequest> Activate(IErosRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> Acquire(IErosRadio radio, CancellationToken cancellationToken)
        {
            return RequestQueue.Enqueue(
                (await NewPodRequest())
                .WithAcquire(radio)
            );
        }

        public Task<IPodRequest> VerifyIdentity(uint lotNumber, uint serialNumber, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> Start()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> Status(StatusRequestType requestType)
        {
            return RequestQueue.Enqueue(
                (await NewPodRequest())
                .WithStatus(requestType)
            );
        }

        public Task<IPodRequest> ConfigureAlerts()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> AcknowledgeAlerts()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> SetBasalSchedule()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> CancelBasal()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> SetTempBasal()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> CancelTempBasal()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> Bolus()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> CancelBolus()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> StartExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> CancelExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> Deactivate()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            RequestQueue.Shutdown();
        }

        private async Task<ErosPodRequest> NewPodRequest()
        {
            var request = await Container.Get<IErosPodRequest>() as ErosPodRequest;
            request.ErosPod = this;
            request.Entity = new PodRequestEntity
            {
                Pod = Entity
            };

            using var context = await RepositoryService.GetContextReadWrite(CancellationToken.None);
            await context.PodRequests.AddAsync(request.Entity);
            await context.Save(CancellationToken.None);
            return request;
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
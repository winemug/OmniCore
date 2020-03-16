using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Eros
{
    public class ErosPod : IErosPod
    {
        public PodEntity Entity { get; set; }
        public PodRunningState RunningState { get; }
        public IPodRequest ActiveRequest { get; }

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ITaskQueue TaskQueue;
        private readonly ICorePodService PodService;

        public ErosPod(ICoreContainer<IServerResolvable> container,
            ICorePodService podService,
            ITaskQueue taskQueue)
        {
            PodService = podService;
            Container = container;
            TaskQueue = taskQueue;
            RunningState = new PodRunningState();
        }
        public Task Archive()
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<IPodRequest>> GetActiveRequests()
        {
            throw new System.NotImplementedException();
        }
        public async Task<IPodRequest> Activate(IRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> Acquire(IRadio radio, CancellationToken cancellationToken)
        {
            Entity.Radios.Clear();

            return (IPodRequest) TaskQueue.Enqueue(
                (await NewPodRequest())
                .WithAcquire(radio as IErosRadio)
            );
        }

        public async Task<IPodRequest> VerifyIdentity(uint lotNumber, uint serialNumber, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public async Task<IPodRequest> Start()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> Status(StatusRequestType requestType)
        {
            return (IPodRequest) TaskQueue.Enqueue(
                (await NewPodRequest())
                .WithStatus(requestType)
                );
        }

        public async Task<IPodRequest> ConfigureAlerts()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> AcknowledgeAlerts()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> SetBasalSchedule()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> CancelBasal()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> SetTempBasal()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> CancelTempBasal()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> Bolus()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> CancelBolus()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> StartExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> CancelExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> Deactivate()
        {
            throw new NotImplementedException();
        }

        public async Task StartMonitoring()
        {
            await StartStateMonitoring();
            TaskQueue.Startup();
        }
        private async Task<ErosPodRequest> NewPodRequest()
        {
            var request = Container.Get<IPodRequest>() as ErosPodRequest;
            request.Pod = this;
            request.Entity = new PodRequestEntity
            {
                Pod = Entity
            };

            var context = Container.Get<IRepositoryContext>();
            await context.PodRequests.AddAsync(request.Entity);
            await context.Save(CancellationToken.None);
            return request;
        }

        private async Task StartStateMonitoring()
        {
            var context = Container.Get<IRepositoryContext>();
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
            PodState state = PodState.Unknown;
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
            address |= (uint)buffer[0] << 16;
            address |= (uint)buffer[1] << 8;
            address |= (uint)buffer[2];
            return address;
        }
        public void Dispose()
        {
            TaskQueue.Shutdown();
        }
    }
}
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
        private readonly ErosRequestQueue RequestQueue;
        private readonly ICorePodService PodService;
        private readonly ICoreRepositoryService RepositoryService;

        public ErosPod(ICoreContainer<IServerResolvable> container,
            ICoreRepositoryService repositoryService,
            ICorePodService podService,
            ErosRequestQueue requestQueue)
        {
            RepositoryService = repositoryService;
            PodService = podService;
            Container = container;
            RequestQueue = requestQueue;
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
        public Task<IPodRequest> Activate(IRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IPodRequest> Acquire(IRadio radio, CancellationToken cancellationToken)
        {
            return RequestQueue.Enqueue(
                (await NewPodRequest())
                .WithAcquire(radio as IErosRadio)
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

        public async Task StartMonitoring()
        {
            await StartStateMonitoring();
            RequestQueue.Startup();
        }
        private async Task<ErosPodRequest> NewPodRequest()
        {
            var request = Container.Get<IErosPodRequest>() as ErosPodRequest;
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
            RequestQueue.Shutdown();
        }
    }
}
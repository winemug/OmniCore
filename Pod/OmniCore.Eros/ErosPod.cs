using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Eros.Annotations;
using OmniCore.Model.Constants;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Eros
{
    public class ErosPod : IPod
    {
        public PodEntity Entity { get; set; }
        public PodRunningState RunningState { get; }
        public IPodRequest ActiveRequest { get; }

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly IRepositoryService RepositoryService;
        private readonly ITaskQueue TaskQueue;
        private readonly IRadioService RadioService;
        private IRadio Radio;

        public ErosPod(ICoreContainer<IServerResolvable> container,
            ITaskQueue taskQueue,
            IRadioService radioService)
        {
            Container = container;
            TaskQueue = taskQueue;
            RadioService = radioService;
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

        public async Task<IPodRequest> RequestPair(uint radioAddress)
        {
            return (IPodRequest) TaskQueue.Enqueue(
                (await NewPodRequest())
                    .WithPair(radioAddress)
            );
        }

        public Task<IPodRequest> RequestPrime()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestInsert()
        {
            throw new System.NotImplementedException();
        }

        public async Task<IPodRequest> RequestStatus(StatusRequestType requestType)
        {
            return (IPodRequest) TaskQueue.Enqueue(
                (await NewPodRequest())
                .WithStatus(requestType)
            );
        }

        public Task<IPodRequest> RequestConfigureAlerts()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestAcknowledgeAlerts()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestSetBasalSchedule()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestSetTempBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelTempBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestStartExtendedBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelExtendedBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestDeactivate()
        {
            throw new System.NotImplementedException();
        }

        public async Task StartMonitoring()
        {
            Radio = await RadioService.ListRadios().FirstOrDefaultAsync(r => r.Entity.Id == Entity.Radio.Id);
            Radio.StartMonitoring();
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
        public void Dispose()
        {
            TaskQueue.Shutdown();
        }
    }
}
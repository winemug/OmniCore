using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Eros.Annotations;
using OmniCore.Model.Constants;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Common.Data.Entities;
using OmniCore.Model.Interfaces.Common.Data.Repositories;

namespace OmniCore.Eros
{
    public class ErosPod : IPod
    {

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly IPodRequestRepository PodRequestRepository;
        private readonly ITaskQueue TaskQueue;

        public ErosPod(ICoreContainer<IServerResolvable> container,
            IPodRequestRepository podRequestRepository,
            ITaskQueue taskQueue)
        {
            Container = container;
            PodRequestRepository = podRequestRepository;
            TaskQueue = taskQueue;
        }

        public IPodEntity Entity { get; set; }
        public IPodRequest ActiveRequest { get; }

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

        public async Task StartQueue()
        {
            TaskQueue.Startup();
        }

        public async Task StopQueue()
        {
            TaskQueue.Shutdown();
        }

        private async Task<ErosPodRequest> NewPodRequest()
        {
            var request = Container.Get<IPodRequest>() as ErosPodRequest;
            request.Pod = this;

            request.Entity = PodRequestRepository.New();
            request.Entity.Pod = this.Entity;
            await PodRequestRepository.Create(request.Entity, CancellationToken.None);
            return request;
        }

#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    }
}
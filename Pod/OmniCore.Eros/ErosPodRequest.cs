using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Requests;
using OmniCore.Services;

namespace OmniCore.Eros
{
    public abstract class ErosPodRequest : IPodRequest
    {
        private readonly ISubject<bool> CanCancelSubject = new BehaviorSubject<bool>(true);
        private readonly ISubject<TaskResult> ResultSubject = new AsyncSubject<TaskResult>();
        private readonly ISubject<TaskState> StateSubject = new BehaviorSubject<TaskState>(TaskState.Scheduled);
        private readonly CancellationTokenSource TaskCancellationSource = new CancellationTokenSource();
        public CancellationToken CancellationToken => TaskCancellationSource.Token;

        protected readonly IRepositoryService RepositoryService;
        protected PodRequestEntity Entity;

        protected ErosPod Pod;
        
        public ErosPodRequest(IRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
        }

        public ITaskProgress TaskProgress { get; }

        public void Cancel()
        {
            TaskCancellationSource.Cancel();
        }

        public IObservable<bool> WhenCanCancelChanged()
        {
            return CanCancelSubject.AsObservable();
        }

        public IObservable<TaskState> WhenStateChanged()
        {
            return StateSubject.AsObservable();
        }

        public IObservable<TaskResult> WhenResultReceived()
        {
            return ResultSubject.AsObservable();
        }

        public async Task ExecuteRequest(IErosRadio radio)
        {
            StateSubject.OnNext(TaskState.Running);
            try
            {
                await ExecuteRequestInternal(radio, TaskCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                ResultSubject.OnNext(TaskResult.Canceled);
                ResultSubject.OnCompleted();
            }
            catch (Exception e)
            {
                ResultSubject.OnError(e);
                ResultSubject.OnNext(TaskResult.Failed);
                ResultSubject.OnCompleted();
            }

            StateSubject.OnNext(TaskState.Finished);
            StateSubject.OnCompleted();
        }

        public void Dispose()
        {
            TaskCancellationSource?.Dispose();
        }

        public IPodRequest ForPod(IPod pod)
        {
            Pod = pod as ErosPod;
            return this;
        }

        public async Task<IPodRequest> Submit(CancellationToken cancellationToken)
        {
            if (Entity != null)
                throw new OmniCoreWorkflowException(FailureType.Internal, "Already submitted");

            try
            {
                Entity = new PodRequestEntity();
                Entity.Pod = Pod.Entity;
                Entity.State = TaskState.Scheduled;
                Entity.Result = TaskResult.Pending;
                using var context = await RepositoryService.GetContextReadWrite(cancellationToken);
                await context.Save(cancellationToken);
            }
            catch
            {
                throw;
            }

            return await Pod.RequestQueue.Enqueue(this);            
        }

        public ErosPodRequest ForPod(ErosPod pod)
        {
            Pod = pod;
            return this;
        }
        

        protected abstract Task ExecuteRequestInternal(IErosRadio radio, CancellationToken cancellationToken);

        // private async Task ExecuteRequestInternal(CancellationToken cancellationToken)
        // {
        //     var radio = RadioOverride;
        //     if (radio == null)
        //     {
        //         //TODO: radio from radioentity
        //     }
        //
        //     var options = radio.Options;
        //
        //     if (TransmissionPowerOverride.HasValue)
        //         options.Amplification = TransmissionPowerOverride.Value;
        //
        //     var response = await radio.GetResponse(this, cancellationToken, options);
        //     var responseEntity = await ParseResponse(response);
        //
        //     using var context = await RepositoryService.GetContextReadWrite(cancellationToken);
        //     Entity.Responses.Add(responseEntity);
        //     await context.Save(cancellationToken);
        // }
    }
}
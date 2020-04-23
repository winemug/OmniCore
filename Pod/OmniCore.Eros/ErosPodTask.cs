using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Services;

namespace OmniCore.Eros
{
    public class ErosPodTask : IPodTask
    {
        private readonly ISubject<bool> CanCancelSubject = new BehaviorSubject<bool>(true);
        private readonly ISubject<TaskResult> ResultSubject = new AsyncSubject<TaskResult>();
        private readonly ISubject<TaskState> StateSubject = new BehaviorSubject<TaskState>(TaskState.Scheduled);
        private readonly CancellationTokenSource TaskCancellationSource = new CancellationTokenSource();

        public IPodRequest Request { get; private set; }
        public IPodResponse Response { get; private set; }

        public ErosPodTask()
        {
        }

        public ITaskProgress TaskProgress { get; }

        public IPodTask WithRequest(IPodRequest request)
        {
            Request = request;
            return this;
        }

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

        public async Task ExecuteRequest()
        {
            StateSubject.OnNext(TaskState.Running);
            try
            {
                //await ExecuteRequestInternal(TaskCancellationSource.Token);
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
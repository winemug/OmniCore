using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
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

        protected ErosPod Pod;
        
        public ErosPodRequest()
        {
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

        public async Task ExecuteRequest()
        {
            StateSubject.OnNext(TaskState.Running);
            try
            {
                await ExecuteRequestInternal(TaskCancellationSource.Token);
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

        public IPodRequest QueueExecution()
        {
            throw new NotImplementedException();
        }

        protected abstract Task ExecuteRequestInternal(CancellationToken cancellationToken);

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

    public class ErosPodActivationRequest : ErosPodRequest, IPodActivationRequest
    {
        public IPodActivationRequest WithRadio(IErosRadio radio)
        {
            throw new NotImplementedException();
        }

        public IPodActivationRequest WithNewRadioAddress(uint radioAddress)
        {
            throw new NotImplementedException();
        }

        public IPodActivationRequest WithPairAndPrime()
        {
            throw new NotImplementedException();
        }

        public IPodActivationRequest WithInjectAndStart(IDeliverySchedule deliverySchedule)
        {
            throw new NotImplementedException();
        }

        public IPodActivationRequest WithDeactivate()
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteRequestInternal(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
    public class ErosPodScheduledDeliveryRequest : ErosPodRequest, IPodScheduledDeliveryRequest
    {
        public IPodScheduledDeliveryRequest WithDeliverySchedule(IDeliverySchedule schedule)
        {
            throw new NotImplementedException();
        }

        public IPodScheduledDeliveryRequest WithTimeOffset(DateTimeOffset timeOffset)
        {
            throw new NotImplementedException();
        }

        public IPodScheduledDeliveryRequest WithTemporaryRate(decimal hourlyRateUnits, TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        public IPodScheduledDeliveryRequest WithScheduledRate()
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteRequestInternal(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class ErosPodBolusRequest : ErosPodRequest, IPodBolusRequest
    {
        public decimal ImmediateBolusUnits { get; }
        public bool ExtendedBolus { get; }
        public bool ExtendedBolusTotalUnits { get; }
        public TimeSpan ExtendedBolusTotalDuration { get; }
        public IPodBolusRequest WithImmediateBolus(decimal immediateBolusUnits)
        {
            throw new NotImplementedException();
        }

        public IPodBolusRequest WithExtendedBolus(decimal extendedBolusTotalUnits, TimeSpan extendedBolusDuration)
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteRequestInternal(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class ErosPodDeliveryCancellationRequest : ErosPodRequest, IPodDeliveryCancellationRequest
    {
        public bool StopBolusDelivery { get; }
        public bool StopExtendedBolusDelivery { get; }
        public bool StopBasalDelivery { get; }
        public IPodDeliveryCancellationRequest WithCancelImmediateBolus()
        {
            throw new NotImplementedException();
        }

        public IPodDeliveryCancellationRequest WithCancelExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public IPodDeliveryCancellationRequest WithStopBasalDelivery()
        {
            throw new NotImplementedException();
        }

        public IPodDeliveryCancellationRequest CancelAll()
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteRequestInternal(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class ErosPodAlarmRequest : ErosPodRequest, IPodAlarmRequest
    {
        protected override async Task ExecuteRequestInternal(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
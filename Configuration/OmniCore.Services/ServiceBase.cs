using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public abstract class ServiceBase : IService
    {
        private readonly AsyncLock StartStopLock;
        private readonly ISubject<INotifyStatus> ServiceStatusSubject;
        private readonly ISubject<IService> ServiceStartedSubject;
        private readonly ISubject<IService> ServiceStoppedSubject;

        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }
        public IObservable<IService> WhenStarted() => ServiceStartedSubject.AsObservable();
        public IObservable<IService> WhenStopped() => ServiceStoppedSubject.AsObservable();

        protected ServiceBase(params IService[] dependencies)
        {
            StartStopLock = new AsyncLock();
            IsStarted = false;
            IsStopped = true;

            StatusFlag = NotifyStatusFlag.OK;
            StatusMessage = "Service created";
            ServiceStatusSubject = new BehaviorSubject<INotifyStatus>(this);
            ServiceStartedSubject = new Subject<IService>();
            ServiceStoppedSubject = new Subject<IService>();
        }


        public async Task StartService(CancellationToken cancellationToken)
        {
            using var _ = await StartStopLock.LockAsync(cancellationToken);
            if (!IsStarted)
            {
                IsStopped = false;
                await OnStart(cancellationToken);
                IsStarted = true;
                ServiceStartedSubject.OnNext(this);
            }
        }

        public async Task StopService(CancellationToken cancellationToken)
        {
            using var _ = await StartStopLock.LockAsync(cancellationToken);
            if (IsStarted)
            {
                IsStarted = false;
                await OnStop(cancellationToken);
                IsStopped = true;
                ServiceStoppedSubject.OnNext(this);
            }
        }

        public NotifyStatusFlag StatusFlag { get; private set; }
        public string StatusMessage { get; private set; }

        public IObservable<INotifyStatus> WhenStatusUpdated()
        {
            throw new NotImplementedException();
        }

        protected abstract Task OnStart(CancellationToken cancellationToken);
        protected abstract Task OnStop(CancellationToken cancellationToken);

        protected void SetStatus(NotifyStatusFlag flag, string message)
        {
            StatusFlag = flag;
            StatusMessage = message;
            ServiceStatusSubject.OnNext(this);
        }

        public void Dispose()
        {
            using var ssl = StartStopLock.Lock();
        }
    }
}
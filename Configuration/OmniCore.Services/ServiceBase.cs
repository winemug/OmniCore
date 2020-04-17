using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly IService[] Dependencies = new IService[0];
        private readonly ISubject<INotifyStatus> ServiceStatusSubject;
        private ConcurrentDictionary<IService, bool> Dependents;
        private readonly AsyncLock PauseResumeLock;

        private readonly AsyncLock StartStopLock;

        protected ServiceBase(params IService[] dependencies)
        {
            Dependents = new ConcurrentDictionary<IService, bool>();
            StartStopLock = new AsyncLock();
            PauseResumeLock = new AsyncLock();
            IsStarted = false;
            if (dependencies != null)
                Dependencies = dependencies;

            foreach (var dependency in Dependencies) dependency.RegisterDependentServices(this);

            StatusFlag = NotifyStatusFlag.OK;
            StatusMessage = "Service created";
            ServiceStatusSubject = new BehaviorSubject<INotifyStatus>(this);
        }

        public bool IsStarted { get; private set; }
        public bool IsPaused { get; private set; }

        public void RegisterDependentServices(IService[] dependentServices)
        {
            foreach (var dependentService in dependentServices)
                Dependents[dependentService] = false;
        }

        public async Task StartService(CancellationToken cancellationToken)
        {
            using var ssl = await StartStopLock.LockAsync(cancellationToken);
            if (!IsStarted)
            {
                foreach (var dependency in Dependencies) await dependency.StartService(cancellationToken);

                await OnStart(cancellationToken);

                IsStarted = true;
            }
        }

        public virtual Task OnBeforeStopRequest()
        {
            return Task.CompletedTask;
        }

        public async Task StopService(CancellationToken cancellationToken)
        {
            using var ssl = await StartStopLock.LockAsync(cancellationToken);
            if (IsStarted)
            {
                foreach (var dependent in Dependents.Keys) await dependent.StopService(cancellationToken);

                await OnStop(cancellationToken);
                IsStarted = false;
            }
        }

        public async Task PauseService(CancellationToken cancellationToken)
        {
            using var ssl = await StartStopLock.LockAsync(cancellationToken);
            if (IsStarted)
            {
                using var prl = await PauseResumeLock.LockAsync(cancellationToken);
                if (!IsPaused)
                {
                    foreach (var dependent in Dependents.Keys) await dependent.PauseService(cancellationToken);
                    await OnPause(cancellationToken);
                    IsPaused = true;
                }
            }
        }

        public async Task ResumeService(CancellationToken cancellationToken)
        {
            using var ssl = await StartStopLock.LockAsync(cancellationToken);
            if (!IsStarted)
            {
                await StartService(cancellationToken);
            }
            else
            {
                using var prl = await PauseResumeLock.LockAsync(cancellationToken);
                if (IsPaused)
                {
                    await OnResume(cancellationToken);
                    IsPaused = false;
                }
            }
        }

        public IList<IDisposable> Disposables { get; } = new List<IDisposable>();

        public void DisposeDisposables()
        {
            foreach (var disposable in Disposables)
                disposable.Dispose();

            Disposables.Clear();
        }

        public NotifyStatusFlag StatusFlag { get; private set; }
        public string StatusMessage { get; private set; }

        public IObservable<INotifyStatus> WhenStatusUpdated()
        {
            throw new NotImplementedException();
        }

        protected abstract Task OnStart(CancellationToken cancellationToken);
        protected abstract Task OnStop(CancellationToken cancellationToken);
        protected abstract Task OnPause(CancellationToken cancellationToken);
        protected abstract Task OnResume(CancellationToken cancellationToken);

        protected void SetStatus(NotifyStatusFlag flag, string message)
        {
            StatusFlag = flag;
            StatusMessage = message;
            ServiceStatusSubject.OnNext(this);
        }

        public void Dispose()
        {
            using var ssl = StartStopLock.Lock();
            using var prl = PauseResumeLock.Lock();
            Dependents = null;
            DisposeDisposables();
        }
    }
}
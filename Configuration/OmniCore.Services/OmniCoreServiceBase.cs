using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Services
{
    public abstract class OmniCoreServiceBase : ICoreService
    {
        private ConcurrentDictionary<ICoreService, bool> Dependents;

        private AsyncLock StartStopLock;
        private AsyncLock PauseResumeLock;

        public bool IsStarted { get; private set; }
        public bool IsPaused { get; private set; }

        protected abstract Task OnStart(CancellationToken cancellationToken);
        protected abstract Task OnStop(CancellationToken cancellationToken);
        protected abstract Task OnPause(CancellationToken cancellationToken);
        protected abstract Task OnResume(CancellationToken cancellationToken);
        private readonly ICoreService[] Dependencies = new ICoreService[0];

        protected OmniCoreServiceBase(params ICoreService[] dependencies)
        {
            Dependents = new ConcurrentDictionary<ICoreService, bool>();
            StartStopLock = new AsyncLock();
            PauseResumeLock = new AsyncLock();
            IsStarted = false;
            if (dependencies != null)
                Dependencies = dependencies;

            foreach (var dependency in Dependencies)
            {
                dependency.RegisterDependentServices(this);
            }
        }

        public void RegisterDependentServices(ICoreService[] dependentServices)
        {
            foreach(var dependentService in dependentServices)
                Dependents[dependentService] = false;
        }

        public async Task StartService(CancellationToken cancellationToken)
        {
            using var ssl = await StartStopLock.LockAsync(cancellationToken);
            if (!IsStarted)
            {
                foreach (var dependency in Dependencies)
                {
                    await dependency.StartService(cancellationToken);
                }

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
                foreach (var dependent in Dependents.Keys)
                {
                    await dependent.StopService(cancellationToken);
                }

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
                    foreach (var dependent in Dependents.Keys)
                    {
                        await dependent.PauseService(cancellationToken);
                    }
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

        public void Dispose()
        {
            using var ssl = StartStopLock.Lock();
            using var prl = PauseResumeLock.Lock();
            Dependents = null;
            DisposeDisposables();
        }

        public IList<IDisposable> Disposables { get; } = new List<IDisposable>();
        public void DisposeDisposables()
        {
            foreach(var disposable in Disposables)
                disposable.Dispose();

            Disposables.Clear();
        }
    }
}

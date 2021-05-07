using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Utilities
{
    public class Lease<T> : ILease<T> where T : ILeaseable<T>
    {
        private static readonly ConcurrentDictionary<T, AsyncLock>
            LeaseLocks = new ConcurrentDictionary<T, AsyncLock>();

        private readonly IDisposable LeaseDisposable;

        private bool Disposed;

        private Lease(T instance, IDisposable leaseDisposable)
        {
            LeaseDisposable = leaseDisposable;
            Instance = instance;
            Instance.OnLease = true;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                Instance.OnLease = false;
                LeaseDisposable.Dispose();
            }
        }

        public T Instance { get; }

        public static async Task<Lease<T>> NewLease(T instance, CancellationToken cancellationToken)
        {
            var leaseLock = LeaseLocks.GetOrAdd(instance, leaseable => new AsyncLock());

            var leaseDisposable = await leaseLock.LockAsync(cancellationToken);

            return new Lease<T>(instance, leaseDisposable);
        }
    }
}
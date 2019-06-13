using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Utilities
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable<T> Sync<T>(this Task<T> t)
        {
            return t.ConfigureAwait(true);
        }

        public static ConfiguredTaskAwaitable Sync(this Task t)
        {
            return t.ConfigureAwait(true);
        }

        public static ConfiguredTaskAwaitable<T> NoSync<T>(this Task<T> t)
        {
            return t.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable NoSync(this Task t)
        {
            return t.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<T> SyncTo<T>(this Task<T> t, SynchronizationContext context)
        {
            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);
            var ret = t.ConfigureAwait(true);
            SynchronizationContext.SetSynchronizationContext(previousContext);
            return ret;
        }

        public static ConfiguredTaskAwaitable SyncTo(this Task t, SynchronizationContext context)
        {
            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);
            var ret = t.ConfigureAwait(true);
            SynchronizationContext.SetSynchronizationContext(previousContext);
            return ret;
        }
    }
}

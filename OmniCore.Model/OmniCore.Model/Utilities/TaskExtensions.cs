using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Utilities
{
    // await(?!(.+\.(No)?Sync\())(.+)
    public static class TaskExtensions
    {
        public static T ExecuteSynchronously<T>(this Task<T> t)
        {
            if (t.Status == TaskStatus.Created)
                t.Start(TaskScheduler.FromCurrentSynchronizationContext());
            t.Wait();
            if (t.Exception != null)
                throw t.Exception;
            return t.Result;
        }

        public static void ExecuteSynchronously(this Task t)
        {
            if (t.Status == TaskStatus.Created)
                t.Start(TaskScheduler.FromCurrentSynchronizationContext());
            t.Wait();
            if (t.Exception != null)
                throw t.Exception;
        }

        //public async static Task<T> AwaitSync<T>(this Task<T> t)
        //{
        //    return await t.ConfigureAwait(true);
        //}

        //public async static void AwaitSync(this Task t)
        //{
        //    await t.ConfigureAwait(true);
        //}

        //public async static Task<T> Sync<T>(this Task<T> t)
        //{
        //    return await t.ConfigureAwait(true);
        //}

        //public async static void Sync(this Task t)
        //{
        //    await t.ConfigureAwait(true);
        //}

        //public async static Task<T> Sync<T>(this Task<T> t, SynchronizationContext context)
        //{
        //    var previousContext = SynchronizationContext.Current;
        //    SynchronizationContext.SetSynchronizationContext(context);
        //    var ret = await t.ConfigureAwait(true);
        //    SynchronizationContext.SetSynchronizationContext(previousContext);
        //    return ret;
        //}

        //public static ConfiguredTaskAwaitable Sync(this Task t, SynchronizationContext context)
        //{
        //    var previousContext = SynchronizationContext.Current;
        //    SynchronizationContext.SetSynchronizationContext(context);
        //    var ret = t.ConfigureAwait(true);
        //    SynchronizationContext.SetSynchronizationContext(previousContext);
        //    return ret;
        //}

        //public static ConfiguredTaskAwaitable<T><T>(this Task<T> t)
        //{
        //    return t.ConfigureAwait(false);
        //}

        //public static ConfiguredTaskAwaitable(this Task t)
        //{
        //    return t.ConfigureAwait(false);
        //}
    }
}

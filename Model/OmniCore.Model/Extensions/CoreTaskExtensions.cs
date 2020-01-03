using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Extensions
{
    public static class CoreTaskExtensions
    {
        public static T WaitAndGetResult<T>(this Task<T> task, CancellationToken cancellationToken,
            bool continueOnCapturedContext = true)
        {
            task.ConfigureAwait(continueOnCapturedContext);
            task.Start();
            task.Wait(cancellationToken);
            if (task.IsCanceled)
            {
                throw new OperationCanceledException();
            }

            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    throw task.Exception;
                }
                throw new InvalidOperationException();
            }
            
            return task.Result;
        }
    }
}
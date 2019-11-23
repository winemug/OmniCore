using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Extensions
{
    public static class SemaphoreExtensions
    {
        public static async Task<bool> WaitAsyncCancellable(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            try
            {
                await semaphore.WaitAsync(cancellationToken);
                return true;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid
{
    public interface IActivityContext
    {
        Task<IForegroundTaskService> GetForegroundTaskService(CancellationToken cancellationToken);
    }
}
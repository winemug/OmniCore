using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IDashPodProvider 
    {
        Task<IList<IErosPod>> ActivePods(CancellationToken cancellationToken);
    }
}
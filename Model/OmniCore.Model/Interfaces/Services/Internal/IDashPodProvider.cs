using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Base;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IDashPodProvider : IServerResolvable
    {
        Task<IList<IPodEros>> ActivePods(CancellationToken cancellationToken);
    }
}
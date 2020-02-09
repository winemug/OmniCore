using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Server
{
    public interface IErosPodProvider : IServerResolvable
    {
        Task<IList<IPodEros>> ActivePods(CancellationToken cancellationToken);
    }
}
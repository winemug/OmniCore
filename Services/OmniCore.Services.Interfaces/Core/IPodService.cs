using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services.Interfaces.Core;

public interface IPodService : ICoreService
{
    Task<IPod> GetPodAsync();

    Task<IPodConnection> GetConnectionAsync(
        IPod pod,
        CancellationToken cancellationToken = default);
}
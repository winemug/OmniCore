using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces;

public interface IPodService
{
    void Start();
    void Stop();
    Task<IPod> GetPodAsync();

    Task<IPodConnection> GetConnectionAsync(
        IPod pod,
        CancellationToken cancellationToken = default);
}
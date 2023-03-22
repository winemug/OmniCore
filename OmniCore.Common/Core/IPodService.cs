using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services.Interfaces.Core;

public interface IPodService : ICoreService
{
    Task ImportPodAsync(Guid id,
        uint radioAddress, int unitsPerMilliliter,
        MedicationType medicationType,
        uint Lot,
        uint Serial,
        uint activeFixedBasalRateTicks);
    Task<IPod> GetPodAsync(Guid id);
    Task<List<IPod>> GetPodsAsync();

    Task<IPodConnection> GetConnectionAsync(
        IPod pod,
        CancellationToken cancellationToken = default);
}
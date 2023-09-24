using OmniCore.Common.Pod;
using OmniCore.Shared.Enums;

namespace OmniCore.Client.Interfaces.Services;

public interface IPodService
{
    Task<Guid> NewPodAsync(
        Guid profileId,
        int unitsPerMilliliter,
        MedicationType medicationType);

    Task RemovePodAsync(Guid podId, DateTimeOffset? removeTime = null);

    Task ImportPodAsync(
        Guid profileId,
        uint radioAddress, int unitsPerMilliliter,
        MedicationType medicationType,
        uint lot,
        uint serial);

    Task<List<IPodModel>> GetPodsAsync(Guid? profileId = null);
    Task<IPodModel?> GetPodAsync(Guid podId);

    Task<IPodConnection> GetConnectionAsync(
        IPodModel podModel,
        CancellationToken cancellationToken = default);
}
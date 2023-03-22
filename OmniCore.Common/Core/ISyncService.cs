namespace OmniCore.Services.Interfaces.Core;

public interface ISyncService : ICoreService
{
    Task SyncPodMessage(Guid podId, int recordIndex);
    Task SyncPod(Guid podId);
}
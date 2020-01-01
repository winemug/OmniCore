using System.Threading;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreClient
    {
        ICoreContainer Container { get; }
        ICoreServicesConnection ServicesConnection { get; }
        SynchronizationContext UiSynchronizationContext { get; }
    }
}
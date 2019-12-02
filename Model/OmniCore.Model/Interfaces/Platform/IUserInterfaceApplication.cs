using System.Threading;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IUserInterfaceApplication
    {
        SynchronizationContext SynchronizationContext { get; }
    }
}
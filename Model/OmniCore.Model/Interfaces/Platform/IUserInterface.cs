using System.Threading;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IUserInterface
    {
        SynchronizationContext SynchronizationContext { get; }
    }
}
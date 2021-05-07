using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface ILockable : IDisposable
    {
        Task<ILockable> Lock(CancellationToken cancellationToken);

        bool IsLocked();
        void ThrowIfUnlocked();
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ILockable : IDisposable
    {
        Task<ILockable> Lock(CancellationToken cancellationToken);

        bool IsLocked();
        void ThrowIfUnlocked();
    }
}

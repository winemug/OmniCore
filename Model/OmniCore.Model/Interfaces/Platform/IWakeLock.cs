using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IWakeLock : IDisposable
    {
        Task<bool> Acquire(int timeout);
        void Release();
        bool IsAcquired { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Interfaces
{
    public interface IWakeLock : IDisposable
    {
        Task<bool> Acquire(int timeout);
        void Release();
        bool IsAcquired { get; }
    }
}

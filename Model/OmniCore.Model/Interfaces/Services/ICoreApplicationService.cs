using System;
using System.Threading;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreApplicationService : ICoreService
    {
        Version Version { get; }
        string DataPath { get; }
        string StoragePath { get; }
        SynchronizationContext UiSynchronizationContext { get; }
        IDisposable DisplayKeepAwake();
        IDisposable BluetoothKeepAwake();
    }
}

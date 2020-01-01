using System;
using System.Threading;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreApplicationService : ICoreService
    {
        Version Version { get; }
        string DataPath { get; }
        string StoragePath { get; }
        IDisposable DisplayKeepAwake();
        IDisposable BluetoothKeepAwake();
        void StorePreferences((string Key, string Value)[] preferences);
        (string Key, string Value)[] ReadPreferences((string Key, string DefaultValue)[] preferences);
    }
}

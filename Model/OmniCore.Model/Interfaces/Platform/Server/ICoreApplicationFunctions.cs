using System;
using System.Threading;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface ICoreApplicationFunctions : ICoreServerFunctions
    {
        Version Version { get; }
        string DataPath { get; }
        string StoragePath { get; }
        IDisposable BluetoothKeepAwake();
        void StorePreferences((string Key, string Value)[] preferences);
        (string Key, string Value)[] ReadPreferences((string Key, string DefaultValue)[] preferences);
    }
}

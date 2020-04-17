using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Client;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ICommonFunctions : IClientInstance, IServiceInstance
    {
        Version Version { get; }
        string DataPath { get; }
        string StoragePath { get; }
        IDisposable BluetoothLock();
        void StorePreferences((string Key, string Value)[] preferences);
        (string Key, string Value)[] ReadPreferences((string Key, string DefaultValue)[] preferences);

        void Exit();

    }
}
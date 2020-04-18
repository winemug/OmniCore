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
        void Exit();

    }
}
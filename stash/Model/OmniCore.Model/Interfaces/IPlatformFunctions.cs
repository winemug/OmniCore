using System;

namespace OmniCore.Model.Interfaces
{
    public interface IPlatformFunctions 
    {
        Version Version { get; }
        string DataPath { get; }
        string StoragePath { get; }
        IDisposable BluetoothLock();
        void Exit();

    }
}
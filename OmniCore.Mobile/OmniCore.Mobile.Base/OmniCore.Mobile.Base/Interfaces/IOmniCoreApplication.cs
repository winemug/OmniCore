using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Base.Interfaces
{
    public interface IOmniCoreApplication
    {
        string Version { get; }
        Task RunOnMainThread(Func<Task> funcTask);
        Task<T> RunOnMainThread<T>(Func<Task<T>> funcTask);
        Task RunOnMainThread(Action action);
        Task<T> RunOnMainThread<T>(Func<T> func);
        Task<SynchronizationContext> GetMainSyncContext();
        string GetPublicDataPath();
        void Exit();
        IWakeLock NewBluetoothWakeLock(string name);
    }
}

using OmniCore.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Client.Uwp.Platform
{
    public class Application : IOmniCoreApplication
    {
        public string Version => string.Empty;

        public IAppState State => throw new NotImplementedException();

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public Task<SynchronizationContext> GetMainSyncContext()
        {
            throw new NotImplementedException();
        }

        public string GetPublicDataPath()
        {
            throw new NotImplementedException();
        }

        public IWakeLock NewBluetoothWakeLock(string name)
        {
            throw new NotImplementedException();
        }

        public Task RunOnMainThread(Func<Task> funcTask)
        {
            throw new NotImplementedException();
        }

        public Task<T> RunOnMainThread<T>(Func<Task<T>> funcTask)
        {
            throw new NotImplementedException();
        }

        public Task RunOnMainThread(Action action)
        {
            throw new NotImplementedException();
        }

        public Task<T> RunOnMainThread<T>(Func<T> func)
        {
            throw new NotImplementedException();
        }
    }
}

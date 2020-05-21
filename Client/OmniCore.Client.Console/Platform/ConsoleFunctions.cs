using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using Windows.ApplicationModel.Background;
using Nito.AsyncEx;
using OmniCore.Model.Interfaces;

namespace OmniCore.Client.Console.Platform
{
    public class ConsoleFunctions : IPlatformFunctions
    {
        public readonly AsyncManualResetEvent ExitEvent
            = new AsyncManualResetEvent(false);
        
        public Version Version { get => new Version(1,0,0,0); }
        public string DataPath
        {
            get => Directory.GetCurrentDirectory();
        }
        
        public string StoragePath
        {
            get => Directory.GetCurrentDirectory();
        }
        public IDisposable BluetoothLock()
        {
            return Disposable.Empty;
        }

        public void Exit()
        {
            ExitEvent.Set();
        }
    }
}
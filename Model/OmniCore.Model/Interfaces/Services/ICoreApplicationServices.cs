using System;
using System.Threading;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreApplicationServices
    {
        Version Version { get; }
        string DataPath { get; }
        string StoragePath { get; }
        IBackgroundTask CreateBackgroundTask();
        void Shutdown();
        SynchronizationContext UiSynchronizationContext { get; }
        ICoreApplicationLogger ApplicationLogger { get; }
        IObservable<IUserInterface> WhenStarted();
        IObservable<IUserInterface> WhenHibernating();
        IObservable<IUserInterface> WhenResuming();
    }
}

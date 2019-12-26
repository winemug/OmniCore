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
        T CreateView<T>() where T: IView<IViewModel>;
        IDisposable KeepAwake();
        IObservable<ICoreApplicationServices> WhenStarted();
        IObservable<ICoreApplicationServices> WhenHibernating();
        IObservable<ICoreApplicationServices> WhenResuming();
    }
}

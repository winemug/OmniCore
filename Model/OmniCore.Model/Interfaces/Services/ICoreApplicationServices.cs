using System;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreApplicationServices
    {
        Version Version { get; }
        string DataPath { get; }
        string StoragePath { get; }
        IBackgroundTask CreateBackgroundTask();
        IObservable<IUserInterface> WhenStarted();
        IObservable<IUserInterface> WhenHibernating();
        IObservable<IUserInterface> WhenResuming();
    }
}

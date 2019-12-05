using System;
using System.Threading;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IUserInterface
    {
        SynchronizationContext SynchronizationContext { get; }
        //IObservable<IUserInterface> WhenStarting();
        //IObservable<IUserInterface> WhenHibernating();
        //IObservable<IUserInterface> WhenResuming();
    }
}
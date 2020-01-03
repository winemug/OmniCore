using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ITask : IDisposable, IServerResolvable
    {
        bool CanCancel { get; }
        void RequestCancellation();
        IObservable<ITask> WhenCannotCancel();
        IObservable<ITask> WhenStarted();
        IObservable<ITask> WhenFinished();
        IObservable<Exception> WhenFailed();
        IObservable<ITask> WhenCanceled();
        IObservable<ITask> WhenMadeRedundant();
        IObservable<ITask> WhenResultLinked();
        IObservable<ITask> WhenRescheduled();
    }
}

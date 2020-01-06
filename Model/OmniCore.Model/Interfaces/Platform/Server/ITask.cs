using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface ITask : IDisposable, IServerResolvable
    {
        Action<CancellationToken> Action { get; set; }
        Task Run();
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

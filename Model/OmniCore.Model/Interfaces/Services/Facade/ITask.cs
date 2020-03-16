using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface ITask : IDisposable, IServerResolvable
    {
        ITaskProgress Progress { get; }
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

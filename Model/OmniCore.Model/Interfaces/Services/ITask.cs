using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ITask : IDisposable
    {
        bool CanCancel { get; }
        void TryCancel();
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

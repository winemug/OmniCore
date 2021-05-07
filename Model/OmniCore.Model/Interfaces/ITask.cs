using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces
{
    public interface ITask : IDisposable
    {
        ITaskProgress TaskProgress { get; }
        void Cancel();
        IObservable<bool> WhenCanCancelChanged();
        IObservable<TaskState> WhenStateChanged();
        IObservable<TaskResult> WhenResultReceived();
    }
}
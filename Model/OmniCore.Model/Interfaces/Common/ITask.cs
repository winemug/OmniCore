using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface ITask : IDisposable, IServerResolvable
    {
        ITaskProgress Progress { get; }
        void Cancel();
        IObservable<bool> WhenCanCancelChanged();
        IObservable<TaskState> WhenStateChanged();
        IObservable<TaskResult> WhenResultReceived();
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IService : INotifyStatus
    {
        bool IsStarted { get; }
        bool IsStopped { get; }
        Task StartService(CancellationToken cancellationToken);
        Task StopService(CancellationToken cancellationToken);
        IObservable<IService> WhenStarted();
        IObservable<IService> WhenStopped();
    }
}
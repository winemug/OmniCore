using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IBackgroundTask : IDisposable, IServerResolvable
    {
        Task<bool> Run(bool tryRunUninterrupted = false);
        Task<bool> RunScheduled(DateTimeOffset time, bool tryRunUninterrupted = false);
        bool IsScheduled { get; }
        DateTimeOffset ScheduledTime { get; }
        Task<bool> CancelScheduledWait();
    }
}

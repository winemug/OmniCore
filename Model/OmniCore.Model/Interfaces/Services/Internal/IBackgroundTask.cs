using System;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IBackgroundTask : IDisposable
    {
        bool IsScheduled { get; }
        DateTimeOffset ScheduledTime { get; }
        Task<bool> Run(bool tryRunUninterrupted = false);
        Task<bool> RunScheduled(DateTimeOffset time, bool tryRunUninterrupted = false);
        Task<bool> CancelScheduledWait();
    }
}
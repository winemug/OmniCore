using System;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IBackgroundTask : IDisposable
    {
        Task<bool> Run(bool tryRunUninterrupted = false);
        Task<bool> RunScheduled(DateTimeOffset time, bool tryRunUninterrupted = false);
        bool IsScheduled { get; }
        DateTimeOffset ScheduledTime { get; }
        Task<bool> CancelScheduledWait();
    }
}

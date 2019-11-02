using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IBackgroundTask
    {
        void Run(bool tryRunUninterrupted = false);
        void RunScheduled(DateTimeOffset time, bool tryRunUninterrupted = false);
        bool IsScheduled { get; }
        DateTimeOffset ScheduledTime { get; }
        bool CancelScheduledWait();
    }
}

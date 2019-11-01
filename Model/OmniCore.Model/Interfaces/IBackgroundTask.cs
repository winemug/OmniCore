using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IBackgroundTask
    {
        void Run<T>(Action<T> action, bool tryRunUninterrupted = false);
        void RunScheduled<T>(DateTimeOffset time, Action<T> action, bool tryRunUninterrupted = false);
        bool CancelSchedule();
    }
}

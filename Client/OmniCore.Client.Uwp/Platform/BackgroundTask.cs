using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Uwp.Platform
{
    public class BackgroundTask : IBackgroundTask
    {
        public bool IsScheduled => throw new NotImplementedException();

        public DateTimeOffset ScheduledTime => throw new NotImplementedException();

        public bool CancelScheduledWait()
        {
            throw new NotImplementedException();
        }

        public void Run(bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public void RunScheduled(DateTimeOffset time, bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }
    }
}

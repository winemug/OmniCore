using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Mobile.Droid.Platform
{
    public class BackgroundTask : IBackgroundTask
    {
        public bool IsScheduled => throw new NotImplementedException();

        public DateTimeOffset ScheduledTime => throw new NotImplementedException();

        public Task<bool> Run(bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RunScheduled(DateTimeOffset time, bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelScheduledWait()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
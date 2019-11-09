using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Model.Interfaces;

namespace OmniCore.Mobile.Droid
{
    public class BackgroundTask : IBackgroundTask
    {
        public bool IsScheduled => throw new NotImplementedException();

        public DateTimeOffset ScheduledTime => throw new NotImplementedException();

        public async Task<bool> Run(bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RunScheduled(DateTimeOffset time, bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CancelScheduledWait()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
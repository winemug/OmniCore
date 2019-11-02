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
    public class BackgroundTask<T> : IBackgroundTask<T>
    {
        public bool IsScheduled => throw new NotImplementedException();

        public DateTimeOffset ScheduledTime => throw new NotImplementedException();

        public T ActionParameter => throw new NotImplementedException();

        public void Run(bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public void RunScheduled(DateTimeOffset time, bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public bool CancelScheduledWait()
        {
            throw new NotImplementedException();
        }
    }
}
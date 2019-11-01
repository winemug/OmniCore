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
        public void Run<T>(Action<T> action, bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public void RunScheduled<T>(DateTimeOffset time, Action<T> action, bool tryRunUninterrupted = false)
        {
            throw new NotImplementedException();
        }

        public bool CancelSchedule()
        {
            throw new NotImplementedException();
        }
    }
}
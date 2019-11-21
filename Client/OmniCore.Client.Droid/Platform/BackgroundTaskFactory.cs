using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Mobile.Droid.Platform
{
    public class BackgroundTaskFactory : IBackgroundTaskFactory
    {
        public IBackgroundTask CreateBackgroundTask(Action action)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Droid.Services
{
    public class ForegroundTaskServiceBinder : Binder
    {
        public IForegroundTaskService ServiceInstance { get; }
        public ForegroundTaskServiceBinder(IForegroundTaskService foregroundTaskService)
        {
            ServiceInstance = foregroundTaskService;
        }
    }
}
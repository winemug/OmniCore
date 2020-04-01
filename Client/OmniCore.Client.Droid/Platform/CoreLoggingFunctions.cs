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
using Android.Util;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Utilities.Extensions;
using OmniCore.Services;

namespace OmniCore.Mobile.Droid.Platform
{
    public class CoreLoggingFunctions : ICoreLoggingFunctions
    {
        public const string TAG = "OmniCore";
        public void Debug(string message)
        {
            Log.Debug(TAG, message);
            //System.Diagnostics.Debug.WriteLine($"{DateTime.Now} {message}");
        }

        public void Error(string message)
        {
            Log.Error(TAG, message);
        }

        public void Error(string message, Exception e)
        {
            Log.Error(TAG, $"{message}\n{e.AsDebugFriendly()}");
        }

        public void Information(string message)
        {
            Log.Info(TAG, message);
        }

        public void Verbose(string message)
        {
            Log.Verbose(TAG, message);
        }

        public void Warning(string message)
        {
            Log.Warn(TAG, message);
        }

        public void Warning(string message, Exception e)
        {
            Log.Warn(TAG, $"{message}\n{e.AsDebugFriendly()}");
        }
    }
}
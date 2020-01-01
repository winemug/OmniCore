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
using OmniCore.Services;

namespace OmniCore.Mobile.Droid.Services
{
    public class CoreLoggingService : OmniCoreService, ICoreLoggingService
    {
        public const string TAG = "OmniCore";
        public void Debug(string message)
        {
            Log.Debug(TAG, message);
        }

        public void Error(string message)
        {
            Log.Error(TAG, message);
        }

        public void Error(string message, Exception e)
        {
            Log.Error(TAG, FormatExceptionMessage(message, e));
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
            Log.Warn(TAG, FormatExceptionMessage(message,e));
        }

        private string FormatExceptionMessage(string message, Exception e)
        {
            var errMessage = new StringBuilder();
            errMessage.AppendLine(message);
            errMessage.AppendLine($"Exception: {e}");
            while (true)
            {
                errMessage.AppendLine($"Message: {e.Message}");
                errMessage.AppendLine($"Requestor: {e.Source}");
                errMessage.AppendLine($"Trace: {e.StackTrace}");
                e = e.InnerException;
                if (e != null)
                {
                    errMessage.AppendLine($"Inner Exception: {e}");
                }
                else
                    break;
            }
            return errMessage.ToString();
        }

        protected override Task OnStart(CancellationToken cancellationToken)
        {
            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes));
            return Task.CompletedTask;
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnPause(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task OnResume(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
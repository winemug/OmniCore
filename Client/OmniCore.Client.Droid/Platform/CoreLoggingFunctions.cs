using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Util;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Utilities.Extensions;
using OmniCore.Services;
using LogLevel = Microsoft.AppCenter.LogLevel;

namespace OmniCore.Mobile.Droid.Platform
{
    public class CoreLoggingFunctions : ICoreLoggingFunctions
    {
        private readonly Logger Logger;

        public CoreLoggingFunctions(ICoreApplicationFunctions applicationFunctions)
        {
            var logPath = Path.Combine(applicationFunctions.StoragePath,
                "logs");

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            // var configuration = new LoggingConfiguration();
            // var fileTarget = new FileTarget
            // {
            //     Encoding = Encoding.UTF8,
            //     Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",
            //     ArchiveAboveSize = 1024 * 1024 * 16,
            //     MaxArchiveFiles = 8
            // };
            // var logTarget = new AsyncTargetWrapper(fileTarget)
            // {
            //     OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
            //     QueueLimit = 1024
            // };
            //
            // configuration.AddTarget(logTarget);
            // configuration.AddRuleForAllLevels(logTarget);
            //
            // Logger = LogManager.GetCurrentClassLogger();
        }

        public void Debug(string message, [CallerFilePath] string source = "")
        {
            
            Log.Debug("OmniCore", message);
        }

        public void Error(string message, [CallerFilePath] string source = "")
        {
        }

        public void Error(string message, Exception e, [CallerFilePath] string source = "")
        {
        }

        public void Information(string message, [CallerFilePath] string source = "")
        {
        }

        public void Verbose(string message, [CallerFilePath] string source = "")
        {
        }

        public void Warning(string message, [CallerFilePath] string source = "")
        {
        }

        public void Warning(string message, Exception e, [CallerFilePath] string source = "")
        {
        }
    }
}
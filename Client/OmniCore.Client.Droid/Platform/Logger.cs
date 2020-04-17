using System;
using System.IO;
using System.Runtime.CompilerServices;
using Android.Util;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Utilities.Extensions;

namespace OmniCore.Client.Droid.Platform
{
    public class Logger : ILogger
    {
        public Logger()
        {
            // var logPath = Path.Combine(applicationFunctions.StoragePath,
            //     "logs");
            //
            // if (!Directory.Exists(logPath))
            //     Directory.CreateDirectory(logPath);

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
            Log.Debug(LoggingConstants.ClientTag, $"{source} {message}");
        }

        public void Error(string message, [CallerFilePath] string source = "")
        {
            Log.Error(LoggingConstants.ClientTag, $"{source} {message}");
        }

        public void Error(string message, Exception e, [CallerFilePath] string source = "")
        {
            Log.Error(LoggingConstants.ClientTag, $"{source} {message}\n{e.AsDebugFriendly()}");
        }

        public static void FatalError(string message, [CallerFilePath] string source = "")
        {
            Log.Error(LoggingConstants.ClientTag, $"{source} {message}");
        }

        public static void FatalError(string message, Exception e, [CallerFilePath] string source = "")
        {
            Log.Error(LoggingConstants.ClientTag, $"{source} {message}\n{e.AsDebugFriendly()}");
        }

        public void Information(string message, [CallerFilePath] string source = "")
        {
            Log.Info(LoggingConstants.ClientTag, $"{source} {message}");
        }

        public void Verbose(string message, [CallerFilePath] string source = "")
        {
            Log.Verbose(LoggingConstants.ClientTag, $"{source} {message}");
        }

        public void Warning(string message, [CallerFilePath] string source = "")
        {
            Log.Warn(LoggingConstants.ClientTag, $"{source} {message}");
        }

        public void Warning(string message, Exception e, [CallerFilePath] string source = "")
        {
            Log.Error(LoggingConstants.ClientTag, $"{source} {message}\n{e.AsDebugFriendly()}");
        }

    }
}
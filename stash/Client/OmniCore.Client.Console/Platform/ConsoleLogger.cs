using System;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Utilities.Extensions;

namespace OmniCore.Client.Console.Platform
{
    public class ConsoleLogger : ILogger
    {
        public void Verbose(string message, string source = "")
        {
            System.Console.WriteLine($"V {source} {message}");
        }

        public void Debug(string message, string source = "")
        {
            System.Console.WriteLine($"D {source} {message}");
        }

        public void Information(string message, string source = "")
        {
            System.Console.WriteLine($"I {source} {message}");
        }

        public void Warning(string message, string source = "")
        {
            System.Console.WriteLine($"W {source} {message}");
        }

        public void Warning(string message, Exception e, string source = "")
        {
            System.Console.WriteLine($"W {source} {message} {e.AsDebugFriendly()}");
        }

        public void Error(string message, string source = "")
        {
            System.Console.WriteLine($"E {source} {message}");
        }

        public void Error(string message, Exception e, string source = "")
        {
            System.Console.WriteLine($"E {source} {message} {e.AsDebugFriendly()}");
        }
    }
}
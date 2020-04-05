using System;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreLoggingFunctions : ICoreServerFunctions
    {
        void Verbose(string message, string source = "");
        void Debug(string message, string source = "");
        void Information(string message, string source = "");
        void Warning(string message, string source = "");
        void Warning(string message, Exception e, string source = "");
        void Error(string message, string source = "");
        void Error(string message, Exception e, string source = "");
    }
}
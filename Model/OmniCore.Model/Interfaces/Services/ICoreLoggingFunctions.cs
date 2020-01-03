using System;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreLoggingFunctions : ICoreServerFunctions
    {
        void Verbose(string message);
        void Debug(string message);
        void Information(string message);
        void Warning(string message);
        void Warning(string message, Exception e);
        void Error(string message);
        void Error(string message, Exception e);
    }
}

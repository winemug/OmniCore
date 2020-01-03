using System;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Interfaces
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

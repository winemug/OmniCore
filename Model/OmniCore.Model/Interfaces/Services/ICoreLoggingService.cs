using System;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreLoggingService : ICoreService
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

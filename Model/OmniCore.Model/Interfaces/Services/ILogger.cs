using System;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ILogger 
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
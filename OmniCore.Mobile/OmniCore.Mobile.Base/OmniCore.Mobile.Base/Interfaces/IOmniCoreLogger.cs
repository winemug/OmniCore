using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Base.Interfaces
{
    public interface IOmniCoreLogger
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

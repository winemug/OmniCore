using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IApplicationService
    {
        Version Version { get; }
        IApplicationLogger Logger { get; }
        string DataPath { get; }
        string StoragePath { get; }
        void Shutdown();
    }
}

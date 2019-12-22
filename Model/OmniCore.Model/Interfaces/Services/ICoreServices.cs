using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreServices
    {
        Task StartUp();
        Task ShutDown();
        ICoreApplicationLogger ApplicationLogger { get; }
        ICoreApplicationServices CoreApplicationServices { get; }
        ICoreDataServices CoreDataServices { get; }
        ICoreIntegrationServices CoreIntegrationServices { get; }
    }
}

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
        Task Startup();
        Task Shutdown();
        ICoreApplicationServices ApplicationServices { get; }
        ICoreDataServices DataServices { get; }
        ICoreIntegrationServices IntegrationServices { get; }
        ICoreAutomationServices AutomationServices { get; }
    }
}

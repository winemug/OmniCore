using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Plugin.BluetoothLE;

namespace OmniCore.Client.Droid.Services
{
    public class CoreServices : ICoreServices
    {
        public ICoreApplicationServices ApplicationServices { get; }
        public ICoreDataServices DataServices { get; }
        public ICoreIntegrationServices IntegrationServices { get; }
        public ICoreAutomationServices AutomationServices { get; }

        public CoreServices(
            ICoreApplicationServices coreApplicationServices,
            ICoreDataServices coreDataServices,
            ICoreIntegrationServices coreIntegrationServices,
            ICoreAutomationServices coreAutomationServices)
        {
            ApplicationServices = coreApplicationServices;
            DataServices = coreDataServices;
            IntegrationServices = coreIntegrationServices;
            AutomationServices = coreAutomationServices;
        }

        public async Task StartUp()
        {

            var dbPath = Path.Combine(ApplicationServices.DataPath, "oc.db3");
            await DataServices.RepositoryService.Initialize(dbPath, CancellationToken.None);
        }

        public async Task ShutDown()
        {
            await DataServices.RepositoryService.Shutdown(CancellationToken.None);
        }

    }
}
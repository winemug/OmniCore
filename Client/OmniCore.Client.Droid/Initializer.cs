using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid;
using OmniCore.Eros;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository.Sqlite;
using OmniCore.Services;
using OmniCore.Simulation;
using Unity;

namespace OmniCore.Client.Droid
{
    public static class Initializer
    {
        public static ICoreContainer WithAndroidPlatformServices(this ICoreContainer container)
        {
            return container
                .One<ICoreApplicationService, CoreApplicationService>()
                .One<ICoreLoggingService, CoreLoggingService>();
        }

        public static ICoreContainer WithAapsIntegrationService(this ICoreContainer container)
        {
            return container.One<ICoreIntegrationService, AapsIntegrationService>();
        }

        public static ICoreContainer AndroidClientContainer(ICoreClientContext clientContext)
        {
            return new OmniCoreContainer()
                .Existing(clientContext)
                .One<ICoreServicesConnection, CoreServicesConnection>()
                .One<ICoreClient, CoreClient>();
        }

        public static ICoreContainer AndroidServiceContainer(ICoreServices coreServices)
        {
            return new OmniCoreContainer()
                .Existing(coreServices)
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithAapsIntegrationService()
#if EMULATOR
                .WithBleSimulator()
#else
                .WithCrossBleRadioAdapter()
#endif
                .WithSqliteRepositories()
                .WithAndroidPlatformServices();
        }
    }
}
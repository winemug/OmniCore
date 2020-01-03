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
        public static ICoreContainer<IServerResolvable> WithAndroidPlatformServices
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .One<ICoreApplicationService, CoreApplicationService>()
                .One<ICoreLoggingService, CoreLoggingService>();
        }

        public static ICoreContainer<IServerResolvable> WithAapsIntegrationService
            (this ICoreContainer<IServerResolvable> container)
        {
            return container.One<ICoreIntegrationService, AapsIntegrationService>();
        }

        public static ICoreContainer<IClientResolvable> AndroidClientContainer(ICoreClientContext clientContext)
        {
            return new OmniCoreContainer<IClientResolvable>()
                .Existing(clientContext)
                .One<ICoreServicesConnection, CoreServicesConnection>()
                .One<ICoreClient, CoreClient>();
        }

        public static ICoreContainer<IServerResolvable> AndroidServiceContainer(ICoreServices coreServices)
        {
            return new OmniCoreContainer<IServerResolvable>()
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
using OmniCore.Client.Droid;
using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Eros;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Server;
using OmniCore.Model.Interfaces.Services;
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
                .One<ICoreApplicationFunctions, CoreApplicationFunctions>()
                .One<ICoreLoggingFunctions, CoreLoggingFunctions>();
        }

        public static ICoreContainer<IServerResolvable> WithAapsIntegrationService
            (this ICoreContainer<IServerResolvable> container)
        {
            return container.One<ICoreIntegrationService, AapsIntegrationServiceBase>();
        }

        public static ICoreContainer<IClientResolvable> AndroidClientContainer(ICoreClientContext clientContext)
        {
            return new OmniCoreContainer<IClientResolvable>()
                .Existing(clientContext)
                .One<ICoreClientConnection, AndroidServiceConnection>()
                .One<ICoreClient, CoreClient>();
        }

        public static ICoreContainer<IServerResolvable> AndroidServiceContainer(ICoreServiceApi serviceApi,
            ICoreNotificationFunctions notificationFunctions)
        {
            return new OmniCoreContainer<IServerResolvable>()
                .Existing(serviceApi)
                .Existing(notificationFunctions)
                .Many<ICoreNotification, CoreNotification>()
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
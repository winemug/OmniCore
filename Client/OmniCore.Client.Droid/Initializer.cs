using OmniCore.Client.Droid;
using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Eros;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository;
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

        public static ICoreContainer<IServerResolvable> AndroidServiceContainer(ICoreApi api,
            ICoreNotificationFunctions notificationFunctions)
        {
            return new OmniCoreContainer<IServerResolvable>()
                .Existing(api)
                .Existing(notificationFunctions)
                .Many<ICoreNotification, CoreNotification>()
                .WithDefaultServices()
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithAapsIntegrationService()
#if EMULATOR
                .WithBleSimulator()
#else
                .WithCrossBleRadioAdapter()
#endif
                .WithEfCoreRepository()
                .WithAndroidPlatformServices();
        }
    }
}
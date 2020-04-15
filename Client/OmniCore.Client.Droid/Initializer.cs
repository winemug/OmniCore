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

        public static ICoreContainer<IClientResolvable> AndroidClientContainer(ICorePlatformClient platformClient)
        {
            return new CoreContainer<IClientResolvable>()
                .Existing(platformClient)
                .One<ICoreClientConnection, AndroidServiceConnection>();
        }

        public static ICoreContainer<IServerResolvable> AndroidServiceContainer(ICoreApi api,
            ICoreNotificationFunctions notificationFunctions)
        {
            return new CoreContainer<IServerResolvable>()
                .Existing(api)
                .Existing(notificationFunctions)
                .Many<ICoreNotification, CoreNotification>()
                .WithDefaultServices()
                .WithOmnipodEros()
                .WithRileyLinkRadio()
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
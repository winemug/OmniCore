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
        public static IContainer<IClientInstance> AndroidClientContainer(IClientFunctions client)
        {
            return new Container<IClientInstance>()
                .Existing(client)
                .One<ILogger, Logger>()
                .One<IClientConnection, AndroidServiceConnection>();
        }

        public static IContainer<IServiceInstance> AndroidServiceContainer(IServiceFunctions service)
        {
            return new Container<IServiceInstance>()
                .Existing(service)
                .One<ICommonFunctions, CommonFunctions>()
                .One<ILogger, Logger>()
                .WithDefaultServices()
                .WithOmnipodEros()
                .WithRileyLinkRadio()
#if EMULATOR
                .WithBleSimulator()
#else
                .WithCrossBleRadioAdapter()
#endif
                .WithEfCoreRepository();
        }
    }
}
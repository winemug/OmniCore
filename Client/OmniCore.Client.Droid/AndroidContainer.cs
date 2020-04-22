using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Eros;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository;
using OmniCore.Services;
using Unity;

namespace OmniCore.Client.Droid
{
    public static class AndroidContainer
    {
        public static IContainer Instance { get; }
        static AndroidContainer()
        {
            Instance = new Container(new UnityContainer());
            Instance
                .Existing(Instance)
                .One<ICommonFunctions, CommonFunctions>()
                .One<ILogger, Logger>()
                .One<IPlatformConfiguration, PlatformConfiguration>()
                .One<IClientConnection, AndroidServiceConnection>()
                .One<IApi, Api>()
                .WithDefaultServices()
                .WithOmnipodEros()
                .WithRileyLinkRadio()
#if EMULATOR
                .WithBleSimulator()
#else
                .WithCrossBleRadioAdapter()
#endif
                .WithEfCoreRepository()
                .WithXamarinFormsClient();
        }
    }
}
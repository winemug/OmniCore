using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Data;
using OmniCore.Eros;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Radios.RileyLink;
using OmniCore.Simulation;
using Unity;

namespace OmniCore.Client.Droid
{
    public static class Initializer
    {
        public static IUnityContainer SetupDependencies()
        {
            return new UnityContainer()
                .WithOmnipodEros()
                .WithRileyLinkRadio()
#if EMULATOR
                .WithBleSimulator()
#else
                .WithCrossBleAdapter()
#endif
                .WithDefaultDataServices()
                .WithXamarinFormsUserInterface()
                .OnAndroidPlatform();
        }

        public static IUnityContainer OnAndroidPlatform(this IUnityContainer container)
        {
            container.RegisterType<IBackgroundTask, BackgroundTask>();
            container.RegisterSingleton<ICoreApplicationServices, CoreApplicationServices>();
            container.RegisterSingleton<ICoreApplicationLogger, CoreApplicationLogger>();

            container.RegisterSingleton<ICoreServices, CoreServices>();
            container.RegisterSingleton<ICoreServicesProvider, CoreServicesProvider>();

            return container;
        }
    }
}
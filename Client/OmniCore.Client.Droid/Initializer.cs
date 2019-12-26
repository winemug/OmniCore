using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Eros;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Platform;
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
        public static IUnityContainer SetupDependencies()
        {
            return new UnityContainer()
                .WithDefaultServices()
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithAapsIntegration()
#if EMULATOR
                .WithBleSimulator()
#else
                .WithCrossBleAdapter()
#endif
                .WithSqliteRepositories()
                .WithXamarinFormsUserInterface()
                .OnAndroidPlatform();
        }

        public static IUnityContainer OnAndroidPlatform(this IUnityContainer container)
        {
            container.RegisterType<IBackgroundTask, BackgroundTask>();
            container.RegisterSingleton<ICoreApplicationServices, CoreApplicationServices>();
            container.RegisterSingleton<ICoreApplicationLogger, CoreApplicationLogger>();

            container.RegisterSingleton<ICoreServices, DroidCoreService>();
            container.RegisterSingleton<ICoreServicesProvider, DroidCoreServiceConnection>();

            return container;
        }

        public static IUnityContainer WithAapsIntegration(this IUnityContainer container)
        {
            container.RegisterSingleton<IIntegrationService, AapsIntegrationService>(RegistrationConstants.AapsIntegration);
            return container;
        }
    }
}
using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Eros;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces;
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
        public static ICoreContainer OnAndroidPlatform(this ICoreContainer container)
        {
            return container
                .One<ICoreApplicationService, CoreApplicationService>()
                .One<ICoreLoggingService, CoreLoggingService>();
        }

        public static ICoreContainer WithAapsIntegration(this ICoreContainer container)
        {
            return container.One<ICoreIntegrationService, AapsIntegrationService>();
        }
    }
}
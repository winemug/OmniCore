using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using OmniCore.Common.Api;
using OmniCore.Common.Core;
using OmniCore.Common.Platform;
using OmniCore.Common.Pod;
using OmniCore.Common.Radio;
using OmniCore.Framework;
using OmniCore.Framework.Api;
using OmniCore.Framework.Ble;
using OmniCore.Framework.Omnipod;
using OmniCore.Maui.Services;
using OmniCore.Maui.ViewModels;


namespace OmniCore.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Services
            .AddSingleton<AppShell>()
            .AddSingleton<CoreNavigationService>()
            .AddTransient<SetupPermissionsModel>()
            .AddTransient<TestViewModel>()
            .AddTransient<TestViewModel2>()
	        

            .AddSingleton<IAppConfiguration, AppConfiguration>()

            //.AddSingleton<IPodService, PodService>()

            .AddSingleton<IRadioService, RadioService>()
            .AddSingleton<IAmqpService, AmqpService>()
            .AddSingleton<ISyncService, SyncService>()
            .AddHostedService<AmqpService>(serviceProvider => (AmqpService)serviceProvider.GetService<IAmqpService>())
            .AddHostedService<SyncService>(serviceProvider => (SyncService)serviceProvider.GetService<ISyncService>())
            .AddHostedService<RadioService>(serviceProvider => (RadioService)serviceProvider.GetService<IRadioService>())

            .AddTransient<IRadio, Radio>()
            .AddTransient<IRadioConnection, RadioConnection >()

            .AddTransient<IPodModel, PodModel>()
            .AddTransient<IPodConnection, PodConnection>()
            .AddTransient<IPodPacket, PodPacket>()
            .AddTransient<IApiClient, ApiClient>();

        builder.RegisterPlatformServices();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
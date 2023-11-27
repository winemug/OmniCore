using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using OmniCore.Client.Platforms;
using OmniCore.Client.Services;
using OmniCore.Client.Views;

namespace OmniCore.Client;

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
#if DEBUG
		builder.Logging.AddDebug();
#endif

        builder.Services
            .AddTransient<EmptyPage>()

            .AddSingleton<NavigationService>()
            .AddSingleton<AppService>()
            .AddSingleton<AppEventsService>()

            .RegisterPlatformServices();


        return builder.Build();
    }
}

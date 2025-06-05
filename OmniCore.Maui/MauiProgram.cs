using CommunityToolkit.Maui;
using Fonts;
using Microsoft.Extensions.Logging;
using OmniCore.Maui.Models;
using OmniCore.Maui.PageModels;
using OmniCore.Maui.Services;
using HomePageModel = OmniCore.Maui.PageModels.HomePageModel;
using PermissionsPageModel = OmniCore.Maui.PageModels.PermissionsPageModel;

namespace OmniCore.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureMauiHandlers(handlers =>
            {
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });
        

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        builder.Services.AddSingleton<HomePageModel>();
        builder.Services.AddSingleton<PermissionsPageModel>();
        builder.Services.AddSingleton<RadiosPageModel>();

        builder.Services.AddSingleton<BleService>();

        builder.Services.RegisterPlatformServices();
        return builder.Build();
    }
}

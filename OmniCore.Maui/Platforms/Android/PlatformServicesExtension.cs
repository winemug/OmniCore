using OmniCore.Maui.Interfaces;
using OmniCore.Maui.Services;

namespace OmniCore.Maui;

public static class PlatformServicesExtension
{
    public static IServiceCollection RegisterPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformInfo, PlatformInfo>();
        services.AddSingleton<IPlatformService, PlatformService>();
        return services;
    }
}
using OmniCore.Common.Platform;
using OmniCore.Maui.Services;

namespace OmniCore.Maui;

public static partial class MauiAppBuilderExtensions
{
    public static MauiAppBuilder RegisterPlatformServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services
            .AddSingleton<IPlatformInfo, PlatformInfo>()
            .AddSingleton<IPlatformService, PlatformService>();
        return mauiAppBuilder;
    }
}
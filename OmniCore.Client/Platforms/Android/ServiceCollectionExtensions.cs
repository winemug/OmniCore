using OmniCore.Client.Abstractions.Services;
using OmniCore.Client.Platforms.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Platforms;
public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPlatformServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IPlatformForegroundService, PlatformForegroundService>()
            .AddSingleton<IPlatformPermissionService, PlatformPermissionService>();
    }
}

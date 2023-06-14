using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using OmniCore.Common.Api;
using OmniCore.Common.Data;
using OmniCore.Maui.Services;
using OmniCore.Maui.ViewModels;
using OmniCore.Maui.Views;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Platform;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return CreateMauiApp(null, null);
    }

    public static MauiApp CreateMauiApp(IPlatformInfo platformInfo, IPlatformService platformService)
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .RegisterAppServices(platformInfo, platformService)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder,
        IPlatformInfo platformInfo, IPlatformService platformService)
    {
	    var vmvMapper = new ViewModelViewMapper();
	    
	    vmvMapper.AddMapping<TestPage, TestViewModel>(mauiAppBuilder);
        vmvMapper.AddMapping<TestPage2, TestViewModel2>(mauiAppBuilder);
	    
	    mauiAppBuilder.Services.AddSingleton(vmvMapper);
	    mauiAppBuilder.Services.AddSingleton<AppShell>();
        mauiAppBuilder.Services.AddSingleton<IAppConfiguration, AppConfiguration>();
        
        mauiAppBuilder.Services.AddSingleton<IPodService, PodService>();
        mauiAppBuilder.Services.AddSingleton<IRadioService, RadioService>();
        mauiAppBuilder.Services.AddSingleton<IAmqpService, AmqpService>();
        mauiAppBuilder.Services.AddSingleton<ISyncService, SyncService>();
        mauiAppBuilder.Services.AddSingleton<IRaddService, RaddService>();
        mauiAppBuilder.Services.AddSingleton<ICoreService, CoreService>();
        mauiAppBuilder.Services.AddSingleton<RaddService>();
        mauiAppBuilder.Services.AddDbContext<OcdbContext>();
        
        mauiAppBuilder.Services.AddTransient<IRadio, Radio>();
        mauiAppBuilder.Services.AddTransient<IRadioConnection, RadioConnection >();
        mauiAppBuilder.Services.AddTransient<IPodModel, PodModel>();
        mauiAppBuilder.Services.AddTransient<IPodConnection, PodConnection>();
        mauiAppBuilder.Services.AddTransient<IPodPacket, PodPacket>();
        mauiAppBuilder.Services.AddTransient<IApiClient, ApiClient>();

        mauiAppBuilder.Services.AddSingleton(typeof(IPlatformService), platformService);
        mauiAppBuilder.Services.AddSingleton(typeof(IPlatformInfo), platformInfo);

        return mauiAppBuilder;
    }
}

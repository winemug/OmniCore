using Microsoft.Extensions.Logging;
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
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .RegisterAppServices()
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

    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {
	    var vmvMapper = new ViewModelViewMapper();
	    
	    vmvMapper.AddMapping<TestPage, TestViewModel>(mauiAppBuilder);
	    
	    mauiAppBuilder.Services.AddSingleton(vmvMapper);
	    mauiAppBuilder.Services.AddSingleton<AppShell>();
        mauiAppBuilder.Services.AddSingleton<IConfigurationStore, ConfigurationStore>();
        mauiAppBuilder.Services.AddSingleton<IDataService, DataService>();
        mauiAppBuilder.Services.AddSingleton<IPodService, PodService>();
        mauiAppBuilder.Services.AddSingleton<IRadioService, RadioService>();
        mauiAppBuilder.Services.AddSingleton<IAmqpService, AmqpService>();
        mauiAppBuilder.Services.AddSingleton<ICoreService, CoreService>();

        mauiAppBuilder.Services.AddTransient<IRadio, Radio>();
        mauiAppBuilder.Services.AddTransient<IRadioConnection, RadioConnection >();
        mauiAppBuilder.Services.AddTransient<IPod, Pod>();
        mauiAppBuilder.Services.AddTransient<IPodConnection, PodConnection>();
        mauiAppBuilder.Services.AddTransient<IPodMessage, PodMessage>();
        mauiAppBuilder.Services.AddTransient<IPodPacket, PodPacket>();

	    mauiAppBuilder.Services.AddSingleton<IPlatformInfo, AndroidPlatformInfo>();
	    mauiAppBuilder.Services.AddSingleton<IPlatformService, AndroidPlatformService>();

        return mauiAppBuilder;
    }
}

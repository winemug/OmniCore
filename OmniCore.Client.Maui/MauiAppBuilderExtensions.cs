using OmniCore.Common.Api;
using OmniCore.Common.Core;
using OmniCore.Common.Data;
using OmniCore.Common.Pod;
using OmniCore.Common.Radio;
using OmniCore.Framework;
using OmniCore.Framework.Api;
using OmniCore.Framework.Ble;
using OmniCore.Framework.Omnipod;
using OmniCore.Maui.Services;
using OmniCore.Maui.ViewModels;
using OmniCore.Maui.Views;

namespace OmniCore.Maui;

public static partial class MauiAppBuilderExtensions
{
    public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {

        mauiAppBuilder.Services.AddSingleton<CoreNavigationService>();
        
        mauiAppBuilder.Services.AddTransient<SetupPermissionsModel>();
        mauiAppBuilder.Services.AddTransient<TestViewModel>();
        mauiAppBuilder.Services.AddTransient<TestViewModel2>();
	    
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

        return mauiAppBuilder;
    }

}
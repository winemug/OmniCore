using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.Services;
using OmniCore.Client.Mobile.ViewModels;
using OmniCore.Client.Mobile.Views;
using OmniCore.Client.Model;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace OmniCore.Client.Mobile
{
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
                .AddDbContext<MobileDbContext>(optionsBuilder =>
                {
                    optionsBuilder.UseSqlite(
                        "Data Source=" +
                        Path.Join(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "oc.sqlite"));
                })

                .Configure<AmqpEndpointDefinition>(
                    builder.Configuration.GetSection(nameof(AmqpEndpointDefinition)))

                .AddViewViewModel<PermissionsPage, PermissionsViewModel>()
                .AddViewViewModel<AccountLoginPage, AccountLoginViewModel>()
                .AddViewViewModel<ListRadiosPage, ListRadiosViewModel>()

                .AddSingleton<ICoreService, CoreService>()
                .AddSingleton<ISettingsService, SettingsService>()
                .AddSingleton<INavigationService, NavigationService>()

                .AddSingleton<AsyncLazy<IBluetoothLE>>(new AsyncLazy<IBluetoothLE>(() => Task.FromResult(CrossBluetoothLE.Current)))
                .AddSingleton<IBleService, BleService>()
                .AddTransient<IBleDevice, BleDevice>()

                .RegisterPlatformServices();
            return builder.Build();
        }
    }
}
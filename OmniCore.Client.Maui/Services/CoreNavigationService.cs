using OmniCore.Common.Platform;
using OmniCore.Maui.ViewModels;
using OmniCore.Maui.Views;

namespace OmniCore.Maui.Services;

public class CoreNavigationService
{
    private readonly AppShell _appShell;
    private readonly IPlatformInfo _platformInfo;
    private readonly IServiceProvider _serviceProvider;

    public CoreNavigationService(
        AppShell appShell,
        IPlatformInfo platformInfo,
        IServiceProvider serviceProvider)
    {
        _appShell = appShell;
        _platformInfo = platformInfo;
        _serviceProvider = serviceProvider;
    }

    public async Task NavigateToMainPage()
    {
    }

    public async Task<Page> GetMainPage()
    {
        if (!await _platformInfo.VerifyPermissions(true))
            return GetPage<SetupPermissionsPage, SetupPermissionsModel>();
        return GetPage<TestPage, TestViewModel>();
    }

    private Page GetPage<TPage, TModel>()
        where TPage : Page, new()
    {
        var page = new TPage();
        var model = _serviceProvider.GetService<TModel>();
        page.BindingContext = model;
        return page;
    }
}
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Maui.Interfaces;
using OmniCore.Maui.Services;

namespace OmniCore.Maui.PageModels;

public partial class HomePageModel : PageModels.PageModel
{
    private readonly IPlatformService _platformService;

    /// <inheritdoc/>
    public HomePageModel(IPlatformService platformService, TestRunService testRunService)
    {
        _platformService = platformService;
        testRunService.StartTests();
    }

    [RelayCommand]
    private async Task StartService()
    {
        _platformService.StartService();
    }

    [RelayCommand]
    private async Task StopService()
    {
        _platformService.StopService();
    }

}
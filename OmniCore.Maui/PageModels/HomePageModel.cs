using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Maui.Interfaces;

namespace OmniCore.Maui.PageModels;

public partial class HomePageModel(IPlatformService platformService, IPlatformInfo platformInfo)
    : PageModels.PageModel
{
    protected override Task Appearing()
    {
        Debug.WriteLine($"{nameof(HomePageModel)}.{nameof(Appearing)}");
        return Task.CompletedTask;
    }

    protected override Task NavigatedTo()
    {
        Debug.WriteLine($"{nameof(HomePageModel)}.{nameof(NavigatedTo)}");
        return Task.CompletedTask;
    }

    protected override Task NavigatedFrom()
    {
        Debug.WriteLine($"{nameof(HomePageModel)}.{nameof(NavigatedFrom)}");
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartService()
    {
        platformService.StartService();
    }

    [RelayCommand]
    private async Task StopService()
    {
        platformService.StopService();
    }

}
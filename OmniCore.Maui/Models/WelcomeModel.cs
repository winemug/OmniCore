using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui.Models;

public partial class WelcomeModel(IPlatformService platformService, IPlatformInfo platformInfo)
    : PageModel
{
    protected override Task Appearing()
    {
        Debug.WriteLine($"{nameof(WelcomeModel)}.{nameof(Appearing)}");
        return Task.CompletedTask;
    }

    protected override Task NavigatedTo()
    {
        Debug.WriteLine($"{nameof(WelcomeModel)}.{nameof(NavigatedTo)}");
        return Task.CompletedTask;
    }

    protected override Task NavigatedFrom()
    {
        Debug.WriteLine($"{nameof(WelcomeModel)}.{nameof(NavigatedFrom)}");
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
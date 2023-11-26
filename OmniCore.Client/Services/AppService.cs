using OmniCore.Client.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Services;

public class AppService
{
    private readonly NavigationService navigationService;

    public AppService(NavigationService navigationService)
    {
        this.navigationService = navigationService;
    }
    public Task OnWindowCreatedAsync()
    {
        return navigationService.PushViewAsync<EmptyPage>();
    }

    public Task OnWindowActivatedAsync()
    {
        return navigationService.AppWindowActivatedAsync();
    }

    public Task OnWindowDeactivatedAsync()
    {
        return navigationService.AppWindowDeactivatedAsync();
    }

    public Task OnStoppedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnResumedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnDestroyingAsync()
    {
        return Task.CompletedTask;
    }
}

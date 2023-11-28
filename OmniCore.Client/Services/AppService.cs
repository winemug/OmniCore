using OmniCore.Client.Abstractions.Services;
using OmniCore.Client.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Services;

public class AppService : IAppEventsSubscriber
{
    private readonly NavigationService navigationService;

    public AppService(NavigationService navigationService, AppEventsService appEventsService)
    {
        this.navigationService = navigationService;
        appEventsService.Subscribe(this);
    }

    public ValueTask OnWindowCreatedAsync()
    {
        return navigationService.PushViewAsync<EmptyPage>();
    }

    public ValueTask OnWindowActivatedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnWindowDeactivatedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnAppStoppedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnAppResumedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnAppDestroyingAsync()
    {
        return ValueTask.CompletedTask;
    }
}

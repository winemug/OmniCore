using OmniCore.Client.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Services;

public class AppEventsService : IAppEventsSubscriber
{
    public void Subscribe(IAppEventsSubscriber subscriber)
    {

    }

    public void Unsubscribe(IAppEventsSubscriber subscriber)
    {

    }
    public ValueTask OnWindowCreatedAsync()
    {
        return ValueTask.CompletedTask;
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

    public ValueTask OnDestroyingAsync()
    {
        return ValueTask.CompletedTask;
    }
}

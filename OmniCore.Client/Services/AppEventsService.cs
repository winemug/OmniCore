using OmniCore.Client.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Services;

public class AppEventsService
{
    private readonly List<IAppEventsSubscriber> subscriberList;

    public AppEventsService(IAppEventsSubscriber[] subscribers)
    {
        subscriberList = new List<IAppEventsSubscriber>();
    }
    public void Subscribe(IAppEventsSubscriber subscriber)
    {
        subscriberList.Add(subscriber);
    }

    public void Unsubscribe(IAppEventsSubscriber subscriber)
    {
        subscriberList.Remove(subscriber);
    }

    private async ValueTask NotifySubscribers(Action<IAppEventsSubscriber> notifyAction)
    {
        var tasks = new Task[subscriberList.Count];
        for(int i = 0; i < subscriberList.Count; i++)
        {
            tasks[i] = Task.Run(() => notifyAction(subscriberList[i]));
        }
        await Task.WhenAll(tasks);
    }
    public ValueTask OnWindowCreatedAsync()
    {
        return NotifySubscribers(s => s.OnWindowCreatedAsync());
    }

    public ValueTask OnWindowActivatedAsync()
    {
        return NotifySubscribers(s => s.OnWindowActivatedAsync());
    }

    public ValueTask OnWindowDeactivatedAsync()
    {
        return NotifySubscribers(s => s.OnWindowDeactivatedAsync());

    }

    public ValueTask OnAppStoppedAsync()
    {
        return NotifySubscribers(s => s.OnAppStoppedAsync());

    }

    public ValueTask OnAppResumedAsync()
    {
        return NotifySubscribers(s => s.OnAppResumedAsync());

    }

    public ValueTask OnAppDestroyingAsync()
    {
        return NotifySubscribers(s => s.OnAppDestroyingAsync());
    }
}

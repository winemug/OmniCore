
namespace OmniCore.Client.Abstractions.Services
{
    public interface IAppEventsSubscriber
    {
        ValueTask OnAppResumedAsync();
        ValueTask OnAppStoppedAsync();
        ValueTask OnAppDestroyingAsync();
        ValueTask OnWindowActivatedAsync();
        ValueTask OnWindowCreatedAsync();
        ValueTask OnWindowDeactivatedAsync();
    }
}
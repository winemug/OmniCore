using OmniCore.Common.Platform;
using OmniCore.Maui.ViewModels;
using OmniCore.Maui.Views;

namespace OmniCore.Maui.Services;

public class NavigationService
{
    private readonly IServiceProvider _serviceProvider;
    public NavigationPage NavigationPage { get; }
    public INavigation Navigation => this.NavigationPage.Navigation;

    public NavigationService(
        IServiceProvider serviceProvider)
    {
        NavigationPage = new NavigationPage();
        _serviceProvider = serviceProvider;
    }
    
    public Task PushAsync<TView, TModel>() where TView : Page
    {
        var view = _serviceProvider.GetRequiredService<TView>();
        var model = _serviceProvider.GetRequiredService<TModel>();
        view.BindingContext = model;
        return Navigation.PushAsync(view);
    }
}
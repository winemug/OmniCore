using OmniCore.Client.Interfaces;
using OmniCore.Client.Interfaces.Services;

namespace OmniCore.Client.Mobile.Services;

public class NavigationService : INavigationService
{
    public NavigationPage NavigationPage { get; }
    public INavigation Navigation => this.NavigationPage.Navigation;

    private IViewModel? _activeModel;
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(
        IServiceProvider serviceProvider)
    {
        NavigationPage = new NavigationPage();
        _serviceProvider = serviceProvider;
        _activeModel = null;
    }

    public async Task PushViewAsync<TView, TModel>()
        where TView : Page
        where TModel : IViewModel
    {
        await NavigateAway(_activeModel);
        var model = _serviceProvider.GetRequiredService<TModel>();
        var view = _serviceProvider.GetRequiredService<TView>();
        await NavigateTo(model, view);
    }

    public async Task PushDataViewAsync<TView, TModel, TModelData>(TModelData data)
        where TView : Page
        where TModel : IDataViewModel<TModelData>
        where TModelData : notnull
    {
        await NavigateAway(_activeModel);

        var model = _serviceProvider.GetRequiredService<TModel>();
        var view = _serviceProvider.GetRequiredService<TView>();
        await model.LoadDataAsync(data);

        await NavigateTo(model, view);
    }

    public async Task AppWindowActivated()
    {
        if (_activeModel != null)
            await _activeModel.OnResumed();
    }

    public async Task AppWindowDeactivated()
    {
        if (_activeModel != null)
            await _activeModel.OnPaused();
    }

    private async Task NavigateAway(IViewModel? model)
    {
        if (model != null)
        {
            await model.OnNavigatingAway();
            if (model is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            if (model is IDisposable disposable)
                disposable.Dispose();
        }
    }

    private async Task NavigateTo(IViewModel model, Page view)
    {
        await model.BindToView(view);
        await model.OnNavigatingTo();
        _activeModel = model;
        await Navigation.PushAsync(view, true);
    }
}
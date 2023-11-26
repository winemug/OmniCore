using OmniCore.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Services;

public class NavigationService
{
    public NavigationPage NavigationPage { get; }
    public INavigation Navigation => this.NavigationPage.Navigation;

    private ViewModel? _activeModel;
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(
        IServiceProvider serviceProvider)
    {
        NavigationPage = new NavigationPage();
        _serviceProvider = serviceProvider;
        _activeModel = null;
    }

    public async Task PushViewAsync<TView>()
    where TView : Page
    {
        await NavigateAway(_activeModel);
        var view = _serviceProvider.GetRequiredService<TView>();
        await NavigateTo(view);
    }

    public async Task PushViewAsync<TView, TModel>()
        where TView : Page
        where TModel : ViewModel
    {
        await NavigateAway(_activeModel);
        var model = _serviceProvider.GetRequiredService<TModel>();
        var view = _serviceProvider.GetRequiredService<TView>();
        await NavigateTo(model, view);
    }

    public async Task PushDataViewAsync<TView, TModel, TModelData>(TModelData data)
        where TView : Page
        where TModel : DataViewModel<TModelData>
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

    private async Task NavigateAway(ViewModel? model)
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

    private async Task NavigateTo(Page view)
    {
        _activeModel = null;
        await Navigation.PushAsync(view, true);
    }

    private async Task NavigateTo(ViewModel model, Page view)
    {
        await model.BindToView(view);
        await model.OnNavigatingTo();
        _activeModel = model;
        await Navigation.PushAsync(view, true);
    }
}
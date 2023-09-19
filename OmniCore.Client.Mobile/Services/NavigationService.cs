using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OmniCore.Client.Interfaces.Services;
using OmniCore.Client.Mobile.ViewModels;
using OmniCore.Client.Mobile.Views;

namespace OmniCore.Client.Mobile.Services;

public class NavigationService : INavigationService
{
    public NavigationPage NavigationPage { get; }
    public INavigation Navigation => this.NavigationPage.Navigation;

    private IViewModel? _activeModel;
    private Page? _activeView;
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(
        IServiceProvider serviceProvider)
    {
        NavigationPage = new NavigationPage();
        _serviceProvider = serviceProvider;
        _activeModel = null;
        _activeView = null;
    }

    public async Task PushAsync<TView, TModel>()
        where TView : Page
        where TModel : IViewModel
    {
        await PushAsync<TView, TModel>();
    }

    public async Task PushAsync<TView, TModel, TModelData>(TModelData? data = default)
        where TView : Page
        where TModel : IViewModel<TModelData>
    {
        if (_activeModel != null)
        {
            await _activeModel.OnNavigatingAway();
        }

        var model = _serviceProvider.GetRequiredService<TModel>();
        var view = _serviceProvider.GetRequiredService<TView>();

        _activeModel = model;
        _activeView = view;


        if (data != null)
        {
            await model.LoadDataAsync(data);
        }

        await model.BindToView(view);
        await model.OnNavigatingTo();
        await Navigation.PushAsync(view);
    }

    private async Task TryDisposeModel(IViewModel? model)
    {
        if (model == null)
            return;
        if (model is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        if (model is IDisposable disposable)
            disposable.Dispose();
    }
}
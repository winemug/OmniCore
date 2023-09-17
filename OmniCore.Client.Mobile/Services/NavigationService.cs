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

    public async ValueTask PushAsync<TView, TModel>()
        where TView : Page
        where TModel : IViewModel
    {
        var model = _serviceProvider.GetRequiredService<TModel>();
        var view = _serviceProvider.GetRequiredService<TView>();
        if (_activeModel != null)
            await _activeModel.DisposeAsync();
        _activeModel = model;
        _activeView = view;
        
        view.BindingContext = model;
        await model.BindView(view);
        await Navigation.PushAsync(view);
    }

    public async ValueTask PushAsync<TView, TModel, TModelData>(TModelData data)
        where TView : Page
        where TModel : IViewModel<TModelData>
    {
        var model = _serviceProvider.GetRequiredService<TModel>();
        var view = _serviceProvider.GetRequiredService<TView>();
        if (_activeModel != null)
            await _activeModel.DisposeAsync();
        _activeModel = model;
        _activeView = view;

        await model.InitializeAsync(data);
        view.BindingContext = model;
        await model.BindView(view);
        await Navigation.PushAsync(view);
    }
}
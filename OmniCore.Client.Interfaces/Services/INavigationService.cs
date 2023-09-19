using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Interfaces.Services;

public interface INavigationService
{
    NavigationPage NavigationPage { get; }
    INavigation Navigation { get; }

    Task AppWindowActivated();
    Task AppWindowDeactivated();
    Task PushViewAsync<TView, TModel>()
        where TView : Page
        where TModel : IViewModel;
    Task PushDataViewAsync<TView, TModel, TModelData>(TModelData data)
        where TView : Page
        where TModel : IDataViewModel<TModelData>
        where TModelData : notnull;
}

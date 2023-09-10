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

    ValueTask PushAsync<TView, TModel>() where TView : Page
        where TModel : IViewModel;

    ValueTask PushAsync<TView, TModel, TModelData>(TModelData data)
        where TView : Page
        where TModel : IViewModel<TModelData>;

}

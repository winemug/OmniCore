using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Abstractions.Services;

public interface INavigationService
{
    Task AppWindowActivated();
    Task AppWindowDeactivated();
    Task PushViewAsync<TView, TModel>()
        where TView : IView
        where TModel : IViewModel;
    Task PushDataViewAsync<TView, TModel, TModelData>(TModelData data)
        where TView : IView
        where TModel : IDataViewModel<TModelData>
        where TModelData : notnull;
}

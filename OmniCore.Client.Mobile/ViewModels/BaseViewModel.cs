using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OmniCore.Client.Mobile.ViewModels;

public abstract class BaseViewModel<T> : BaseViewModel, IViewModel<T>
{
    public abstract Task LoadDataAsync(T data);
}

public class BaseViewModel : ObservableObject, IViewModel
{
    public virtual Task OnNavigatingTo()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnNavigatingAway()
    {
        return Task.CompletedTask;
    }

    public virtual Task BindToView(Page page)
    {
        page.BindingContext = this;
        return Task.CompletedTask;
    }
}

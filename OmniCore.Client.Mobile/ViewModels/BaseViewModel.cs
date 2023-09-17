using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OmniCore.Client.Mobile.ViewModels;

public abstract class BaseViewModel<T> : BaseViewModel, IViewModel<T>
{
    public abstract ValueTask InitializeAsync(T data);
}

public class BaseViewModel : ObservableObject, IViewModel
{
    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask BindView(Page page)
    {
        return ValueTask.CompletedTask;
    }
}

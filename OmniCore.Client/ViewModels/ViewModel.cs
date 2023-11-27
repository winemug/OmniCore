using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.ViewModels;

public class ViewModel : ObservableObject
{
    public virtual ValueTask OnResumed() => ValueTask.CompletedTask;
    public virtual ValueTask OnPaused() => ValueTask.CompletedTask;

    public virtual ValueTask OnNavigatingTo() => ValueTask.CompletedTask;

    public virtual ValueTask OnNavigatingAway() => ValueTask.CompletedTask;

    public virtual ValueTask BindToView(Page page)
    {
        page.BindingContext = this;
        return ValueTask.CompletedTask;
    }
}

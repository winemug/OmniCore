using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.ViewModels;

public class ViewModel : ObservableObject
{
    public virtual Task OnResumed() => Task.CompletedTask;
    public virtual Task OnPaused() => Task.CompletedTask;

    public virtual Task OnNavigatingTo() => Task.CompletedTask;

    public virtual Task OnNavigatingAway() => Task.CompletedTask;

    public virtual Task BindToView(Page page)
    {
        page.BindingContext = this;
        return Task.CompletedTask;
    }
}

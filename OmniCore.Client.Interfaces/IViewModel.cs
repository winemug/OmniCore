using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IViewModel<in TData> : IViewModel
{
    Task LoadDataAsync(TData data);
}

public interface IViewModel
{
    Task OnNavigatingTo();
    Task OnNavigatingAway();
    Task BindToView(Page page);
}